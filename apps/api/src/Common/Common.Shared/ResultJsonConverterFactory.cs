using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Milese.Common.Shared;

public class ResultJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType &&
               typeToConvert.GetGenericTypeDefinition() == typeof(Result<,>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var genericArgs = typeToConvert.GetGenericArguments();
        var converterType = typeof(ResultJsonConverter<,>).MakeGenericType(genericArgs[0], genericArgs[1]);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public class ResultJsonConverter<T, TErr> : JsonConverter<Result<T, TErr>>
{
    public override Result<T, TErr> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var isSuccess = root.GetProperty("isSuccess").GetBoolean();

        if (isSuccess)
        {
            var value = JsonSerializer.Deserialize<T>(root.GetProperty("value").GetRawText(), options)!;
            return Result<T, TErr>.Success(value);
        }
        else
        {
            var error = JsonSerializer.Deserialize<TErr>(root.GetProperty("error").GetRawText(), options)!;
            return Result<T, TErr>.Failure(error);
        }
    }

    public override void Write(Utf8JsonWriter writer, Result<T, TErr> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBoolean("isSuccess", value.IsSuccess);

        if (value.IsSuccess)
        {
            writer.WritePropertyName("value");
            JsonSerializer.Serialize(writer, value.Value, options);
        }
        else
        {
            writer.WritePropertyName("error");
            JsonSerializer.Serialize(writer, value.Error, options);
        }

        writer.WriteEndObject();
    }
}
