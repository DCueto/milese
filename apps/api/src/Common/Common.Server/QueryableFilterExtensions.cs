using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Milese.Common.Server;

public static class QueryableFilterExtensions
{
    public static IQueryable<T> WhereIfValue<T, TValue>(
        this IQueryable<T> source,
        TValue? value,
        Func<TValue, Expression<Func<T, bool>>> predicate
    )
        where TValue : struct =>
        value.HasValue ? source.Where(predicate(value.Value)) : source;

    public static IQueryable<T> WhereIfValue<T, TValue>(
        this IQueryable<T> source,
        TValue? value,
        Func<TValue, Expression<Func<T, bool>>> predicate
    )
        where TValue : class =>
        value is not null ? source.Where(predicate(value)) : source;

    public static IQueryable<T> WhereIfValue<T>(
        this IQueryable<T> source,
        string? value,
        Func<string, Expression<Func<T, bool>>> predicate
    ) =>
        !string.IsNullOrWhiteSpace(value) ? source.Where(predicate(value)) : source;

    /// <summary>
    /// Multi-value counterpart of <see cref="WhereIfValue{T}"/>: an empty (or null) selection
    /// means "no restriction", so the filter is skipped. When the entity holds value types,
    /// map the selected values inside the body of <paramref name="predicate"/>.
    /// </summary>
    public static IQueryable<T> WhereIfNotEmpty<T, TValue>(
        this IQueryable<T> source,
        IReadOnlyCollection<TValue>? values,
        Func<IReadOnlyCollection<TValue>, Expression<Func<T, bool>>> predicate
    ) =>
        values is { Count: > 0 } ? source.Where(predicate(values)) : source;
}
