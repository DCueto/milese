using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Milese.Common.Shared;

public class EitherJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType &&
               typeToConvert.GetGenericTypeDefinition() == typeof(Either<,>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var genericArgs = typeToConvert.GetGenericArguments();
        var converterType = typeof(EitherJsonConverter<,>).MakeGenericType(genericArgs[0], genericArgs[1]);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

public class EitherJsonConverter<TLeft, TRight> : JsonConverter<Either<TLeft, TRight>>
{
    public override Either<TLeft, TRight> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var isRight = root.GetProperty("isRight").GetBoolean();

        if (isRight)
        {
            var right = JsonSerializer.Deserialize<TRight>(root.GetProperty("right").GetRawText(), options)!;
            return Either<TLeft, TRight>.FromRight(right);
        }
        else
        {
            var left = JsonSerializer.Deserialize<TLeft>(root.GetProperty("left").GetRawText(), options)!;
            return Either<TLeft, TRight>.FromLeft(left);
        }
    }

    public override void Write(Utf8JsonWriter writer, Either<TLeft, TRight> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteBoolean("isRight", value.IsRight);

        if (value.IsRight)
        {
            writer.WritePropertyName("right");
            JsonSerializer.Serialize(writer, value.Right, options);
        }
        else
        {
            writer.WritePropertyName("left");
            JsonSerializer.Serialize(writer, value.Left, options);
        }

        writer.WriteEndObject();
    }
}
