using System;
using System.Collections.Generic;

namespace Milese.Common.Shared;

/// <summary>
/// Common contract for domain value types that wrap an underlying value of type
/// <typeparamref name="T"/>. Uses the CRTP pattern (<typeparamref name="TSelf"/>
/// is the implementing type itself) so instances can be reconstructed generically,
/// for example by the Entity Framework converter. Exposing the underlying value
/// as <typeparamref name="T"/> lets the converter read and set data generically
/// without knowing the concrete storage type.
/// </summary>
/// <typeparam name="TSelf">The value type implementing this interface.</typeparam>
/// <typeparam name="T">The type of the underlying value the value type wraps.</typeparam>
public interface IValueType<TSelf, T> : IComparable<TSelf>
    where TSelf : IValueType<TSelf, T>
{
    /// <summary>
    /// The underlying <typeparamref name="T"/> representation of the value type.
    /// </summary>
    T Value { get; init; }

    /// <summary>
    /// Orders by the underlying value. Implemented here (default interface
    /// implementation) so every value type is sortable with LINQ (<c>OrderBy</c>,
    /// <c>Sort</c>) without repeating code in each type; otherwise
    /// <see cref="Comparer{T}"/> fails at runtime for not finding
    /// <see cref="IComparable{T}"/>.
    /// </summary>
    int IComparable<TSelf>.CompareTo(TSelf? other) =>
        other is null ? 1 : Comparer<T>.Default.Compare(Value, other.Value);
}
