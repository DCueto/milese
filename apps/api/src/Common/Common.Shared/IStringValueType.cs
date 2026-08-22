namespace Milese.Common.Shared;

/// <summary>
/// Contract for domain value types whose underlying value is a <see cref="string"/>.
/// Specialises <see cref="IValueType{TSelf, T}"/> with <c>T = string</c> and adds
/// the maximum length of the value.
/// </summary>
/// <remarks>
/// Thanks to <see cref="MaxLength"/>, the Entity Framework converter registry
/// can automatically apply the column's maximum length (equivalent to
/// <c>[MaxLength]</c>) without annotating every property.
/// </remarks>
/// <typeparam name="TSelf">The value type implementing this interface.</typeparam>
public interface IStringValueType<TSelf> : IValueType<TSelf, string>
    where TSelf : IStringValueType<TSelf>
{
    /// <summary>
    /// Maximum length allowed for the underlying value. The converter registry
    /// uses it to configure the database column's length.
    /// </summary>
    static abstract int MaxLength { get; }
}
