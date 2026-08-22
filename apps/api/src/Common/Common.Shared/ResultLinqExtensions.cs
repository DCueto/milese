using System;
using System.Threading.Tasks;

#pragma warning disable VSTHRD200 // Use "Async" suffix — Select/SelectMany must keep their LINQ names
#pragma warning disable VSTHRD003 // Avoid awaiting foreign tasks — this is a generic combinator library

namespace Milese.Common.Shared;

/// <summary>
/// Select/SelectMany extensions that enable LINQ query syntax over Result.
/// Example: from x in Result1 from y in Result2 select (x, y)
/// Includes async overloads so synchronous and asynchronous steps (source
/// and/or collection as Task&lt;Result&lt;...&gt;&gt;) can be mixed in the same query.
/// </summary>
public static class ResultLinqExtensions
{
    public static Result<TResult, TErr> Select<T, TErr, TResult>(
        this Result<T, TErr> source,
        Func<T, TResult> selector) =>
        source.Map(selector);

    public static async Task<Result<TResult, TErr>> Select<T, TErr, TResult>(
        this Task<Result<T, TErr>> source,
        Func<T, TResult> selector) =>
        (await source.ConfigureAwait(false)).Map(selector);

    public static Result<TResult, TErr> SelectMany<TSource, TErr, TCollection, TResult>(
        this Result<TSource, TErr> source,
        Func<TSource, Result<TCollection, TErr>> collectionSelector,
        Func<TSource, TCollection, TResult> resultSelector) =>
        source.Map(s => collectionSelector(s).Map(c => resultSelector(s, c)));

    public static async Task<Result<TResult, TErr>> SelectMany<TSource, TErr, TCollection, TResult>(
        this Task<Result<TSource, TErr>> source,
        Func<TSource, Result<TCollection, TErr>> collectionSelector,
        Func<TSource, TCollection, TResult> resultSelector) =>
        (await source.ConfigureAwait(false)).SelectMany(collectionSelector, resultSelector);

    public static async Task<Result<TResult, TErr>> SelectMany<TSource, TErr, TCollection, TResult>(
        this Task<Result<TSource, TErr>> source,
        Func<TSource, Task<Result<TCollection, TErr>>> collectionSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        var s = await source.ConfigureAwait(false);
        if (s.IsFailure)
            return s.Error;

        return (await collectionSelector(s.Value).ConfigureAwait(false)).Map(c => resultSelector(s.Value, c));
    }

    public static async Task<Result<TResult, TErr>> SelectMany<TSource, TErr, TCollection, TResult>(
        this Result<TSource, TErr> source,
        Func<TSource, Task<Result<TCollection, TErr>>> collectionSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        if (source.IsFailure)
            return source.Error;

        return (await collectionSelector(source.Value).ConfigureAwait(false)).Map(c => resultSelector(source.Value, c));
    }
}
