namespace Milese.Common.Shared;

/// <summary>
/// Contract for domain value types whose underlying value is a decimal number
/// (<see cref="decimal"/>). Specialises <see cref="IValueType{TSelf, T}"/> with
/// <c>T = decimal</c> and adds the precision (total number of digits and
/// number of decimal digits) of the value.
/// </summary>
/// <remarks>
/// Thanks to <see cref="Precision"/>, the Entity Framework converter registry
/// can automatically apply the column's precision (equivalent to
/// <c>HasPrecision(precision, scale)</c>) without annotating every property
/// with <c>[Column(TypeName = "decimal(p,s)")]</c>.
/// </remarks>
/// <typeparam name="TSelf">The value type implementing this interface.</typeparam>
public interface INumericValueType<TSelf> : IValueType<TSelf, decimal>
    where TSelf : INumericValueType<TSelf>
{
    /// <summary>
    /// Precision of the underlying value: total number of digits
    /// (<c>Precision</c>) and number of decimal digits (<c>Scale</c>). The
    /// converter registry uses it to configure the database column's precision.
    /// </summary>
    (int Precision, int Scale) Precision { get; }
}
