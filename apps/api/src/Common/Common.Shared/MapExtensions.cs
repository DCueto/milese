using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable VSTHRD200 // Use "Async" suffix — Map must stay Map by design
#pragma warning disable VSTHRD003 // Avoid awaiting foreign tasks — this is a generic combinator library

namespace Milese.Common.Shared;

/// <summary>
/// Inspired by F#'s |> and >>= operators, but expressed as Map overloads.
/// </summary>
public static class MapExtensions
{
    public static void Do<T>(this T source, Action<T> action) =>
        action(source);

    public static TOut Map<T, TOut>(this T source, Func<T, TOut> mapper) =>
        mapper(source);

    public static async Task<TOut> Map<T, TOut>(this Task<T> source, Func<T, TOut> mapper)
    {
        var value = await source.ConfigureAwait(false);
        return mapper(value);
    }

    public static async Task<TOut> Map<T, TOut>(this Task<T> source, Func<T, Task<TOut>> mapper)
    {
        var value = await source.ConfigureAwait(false);
        return await mapper(value).ConfigureAwait(false);
    }

    public static Result<TOut, TErr> Map<T, TErr, TOut>(this Result<T, TErr> source, Func<T, TOut> mapper) =>
        source.IsSuccess
            ? Result<TOut, TErr>.Success(mapper(source.Value))
            : Result<TOut, TErr>.Failure(source.Error);

    public static Result<TOut, TErr> Map<T, TErr, TOut>(this Result<T, TErr> source, Func<T, Result<TOut, TErr>> mapper) =>
        source.IsSuccess
            ? mapper(source.Value)
            : Result<TOut, TErr>.Failure(source.Error);

    public static async Task<Result<TOut, TErr>> Map<T, TErr, TOut>(this Task<Result<T, TErr>> source, Func<T, TOut> mapper)
    {
        var result = await source.ConfigureAwait(false);
        return result.IsSuccess
            ? Result<TOut, TErr>.Success(mapper(result.Value))
            : Result<TOut, TErr>.Failure(result.Error);
    }

    public static async Task<Result<TOut, TErr>> Map<T, TErr, TOut>(this Task<Result<T, TErr>> source, Func<T, Result<TOut, TErr>> mapper)
    {
        var result = await source.ConfigureAwait(false);
        return result.IsSuccess
            ? mapper(result.Value)
            : Result<TOut, TErr>.Failure(result.Error);
    }

    public static async Task<Result<TOut, TErr>> Map<T, TErr, TOut>(this Task<Result<T, TErr>> source, Func<T, Task<Result<TOut, TErr>>> mapper)
    {
        var result = await source.ConfigureAwait(false);
        if (!result.IsSuccess)
            return Result<TOut, TErr>.Failure(result.Error);
        return await mapper(result.Value).ConfigureAwait(false);
    }

    public static async Task<IEnumerable<TOut>> Map<T, TOut>(this IEnumerable<T> source, Func<T, Task<TOut>> mapper)
    {
        var results = new List<TOut>();
        foreach (var item in source)
        {
            results.Add(await mapper(item).ConfigureAwait(false));
        }
        return results;
    }

    public static Result<IEnumerable<TOut>, TErr> Map<T, TErr, TOut>(this IEnumerable<T> source, Func<T, Result<TOut, TErr>> mapper)
    {
        var results = new List<TOut>();
        foreach (var item in source)
        {
            var result = mapper(item);
            if (result.IsFailure)
                return Result<IEnumerable<TOut>, TErr>.Failure(result.Error);
            results.Add(result.Value);
        }
        return Result<IEnumerable<TOut>, TErr>.Success(results);
    }

    public static Result<TOut?, TErr> Map<T, TOut, TErr>(this T? source, Func<T, Result<TOut, TErr>> mapper)
        where TOut : struct
        => source switch
        {
            null => Result<TOut?, TErr>.Success(default),
            { } v => mapper(v).Map(vv => (TOut?)vv)
        };

    /// <summary>
    /// If it has a value, applies the function. If it's null, returns null.
    /// </summary>
    public static TOut? Map<T, TOut>(this T? source, Func<T, TOut?> mapper)
        where T : struct
        where TOut : struct =>
        source.HasValue ? mapper(source.Value) : null;

    public static async Task<Result<TOut, TErr>> MapTask<T, TErr, TOut>(
        this Task<Result<T, TErr>> source,
        Func<T, Task<TOut>> mapper)
    {
        var sourceValue = await source;
        return sourceValue.IsSuccess
            ? await mapper(sourceValue.Value)
            : Result<TOut, TErr>.Failure(sourceValue.Error);
    }

    public static async Task<Result<TOut, TErr>> MapTask<T, TErr, TOut>(
        this Result<T, TErr> source,
        Func<T, Task<TOut>> mapper) =>
        source.IsSuccess
            ? await mapper(source.Value)
            : Result<TOut, TErr>.Failure(source.Error);

    public static Task<Result<TOut, TErr>> MapTask<T, TErr, TOut>(
        this Result<T, TErr> source,
        Func<T, Task<Result<TOut, TErr>>> mapper) =>
        source.IsSuccess
            ? mapper(source.Value)
            : Task.FromResult(Result<TOut, TErr>.Failure(source.Error));

    public static async Task<Result<TOut, TErr>> MapTask<T, TErr, TOut>(
        this Task<Result<T, TErr>> source,
        Func<T, Task<Result<TOut, TErr>>> mapper)
    {
        var sourceValue = await source;
        return sourceValue.IsSuccess
            ? await mapper(sourceValue.Value)
            : Result<TOut, TErr>.Failure(sourceValue.Error);
    }

    public static Task<Result<TOut, TErr>> MapTask<T, TErr, TOut>(
        this T source,
        Func<T, Task<Result<TOut, TErr>>> mapper) => mapper(source);

    public static async Task<Result<TOut, Either<TErr, TErr2>>> Map2Errors<T, TErr, TErr2, TOut>(
        this Result<T, TErr> source,
        Func<T, Task<Result<TOut, TErr2>>> mapper)
    {
        if (!source.IsSuccess)
            return Either<TErr, TErr2>.FromLeft(source.Error);

        var mapped = await mapper(source.Value).ConfigureAwait(false);

        if (!mapped.IsSuccess)
            return Either<TErr, TErr2>.FromRight(mapped.Error);

        return mapped.Value;
    }

    public static async Task<Result<TOut, Either<TErr, TErr2>>> Map2Errors<T, TErr, TErr2, TOut>(
        this Task<Result<T, TErr>> source,
        Func<T, Task<Result<TOut, TErr2>>> mapper)
    {
        var result = await source.ConfigureAwait(false);
        if (!result.IsSuccess)
            return Either<TErr, TErr2>.FromLeft(result.Error);

        var mapped = await mapper(result.Value).ConfigureAwait(false);

        if (!mapped.IsSuccess)
            return Either<TErr, TErr2>.FromRight(mapped.Error);

        return mapped.Value;
    }

    public static TOut? MapNullable<T, TOut>(this T? source, Func<T, TOut?> mapper)
        where T : class
        where TOut : class =>
        source is not null ? mapper(source) : null;

    public static async Task<TOut> Match<T, TErr, TOut>(
        this Task<Result<T, TErr>> source,
        Func<T, TOut> onSuccess,
        Func<TErr, TOut> onFailure)
    {
        var result = await source.ConfigureAwait(false);
        return result.Match(onSuccess, onFailure);
    }

    public static async Task Switch<T, TErr>(
       this Task<Result<T, TErr>> source,
       Action<T> onSuccess,
       Action<TErr> onFailure)
    {
        var result = await source.ConfigureAwait(false);
        var _ = result.Match(
            ok =>
            {
                onSuccess(ok);
                return true;
            },
            fail =>
            {
                onFailure(fail);
                return true;
            });
    }
}
