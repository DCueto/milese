using Milese.Common.Shared;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Milese.Common.Server;

/// <summary>
/// EF Core value converter that persists any value type implementing
/// <see cref="IValueType{TSelf, T}"/> as its underlying representation of type
/// <typeparamref name="T"/>, and reconstructs it when reading from the database.
/// Thanks to this, database entities can declare properties with the domain type
/// (e.g. <c>LessonId</c>) instead of <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="TValueType">
/// The value type to convert. Must implement <see cref="IValueType{TSelf, T}"/>.
/// </typeparam>
/// <typeparam name="T">The type of the underlying value the value type wraps.</typeparam>
public sealed class ValueTypeConverter<TValueType, T> : ValueConverter<TValueType, T>
    where TValueType : IValueType<TValueType, T>, new()
{
    /// <summary>
    /// Initializes a new instance of the converter.
    /// </summary>
    public ValueTypeConverter()
        : base(valueType => valueType.Value, value => FromValue(value))
    {
    }

    // Generic type parameters don't support parameterized constructors; use init-property assignment instead.
    private static TValueType FromValue(T value) => new() { Value = value };
}
