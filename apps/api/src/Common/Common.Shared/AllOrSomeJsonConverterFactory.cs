using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Milese.Common.Shared;

public class AllOrSomeJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType &&
               typeToConvert.GetGenericTypeDefinition() == typeof(AllOrSome<>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var genericArgs = typeToConvert.GetGenericArguments();
        var converterType = typeof(AllOrSomeJsonConverter<>).MakeGenericType(genericArgs[0]);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public class AllOrSomeJsonConverter<T> : JsonConverter<AllOrSome<T>>
{
    public override AllOrSome<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var isAll = root.GetProperty("isAll").GetBoolean();
        if (isAll)
            return AllOrSome<T>.All();

        if (!root.TryGetProperty("values", out var valuesElement))
            throw new JsonException("An AllOrSome with isAll=false must include the 'values' property.");

        var values = JsonSerializer.Deserialize<List<T>>(valuesElement.GetRawText(), options);
        if (values is null || values.Count == 0)
            throw new JsonException("An AllOrSome with isAll=false must include at least one value in 'values'.");

        return AllOrSome<T>.Some(NotEmptyList<T>.FromTrusted(values));
    }

    public override void Write(Utf8JsonWriter writer, AllOrSome<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
#pragma warning disable RS0030 // Serialization needs the explicit discriminator in the JSON.
        writer.WriteBoolean("isAll", value.IsAll);

        if (!value.IsAll)
        {
            writer.WritePropertyName("values");
            JsonSerializer.Serialize(writer, value.Items, options);
        }
#pragma warning restore RS0030

        writer.WriteEndObject();
    }
}
