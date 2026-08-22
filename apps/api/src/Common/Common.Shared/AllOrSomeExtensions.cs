using System;
using System.Linq;

namespace Milese.Common.Shared;

public static class AllOrSomeExtensions
{
    /// <summary>
    /// Applies a validation to each value in the <c>Some</c> branch, staying on the
    /// <see cref="Result{T, TErr}"/> rail. The <c>All</c> branch is propagated without
    /// validating anything.
    /// </summary>
    public static Result<AllOrSome<TOut>, TErr> MapValues<T, TErr, TOut>(
        this AllOrSome<T> source,
        Func<T, Result<TOut, TErr>> mapper
    ) =>
        source.Match(
            () => Result<AllOrSome<TOut>, TErr>.Success(AllOrSome<TOut>.All()),
            values => NotEmptyList<T>.FromTrusted(values)
                .Map(mapper)
                .Map(mapped => AllOrSome<TOut>.Some(NotEmptyList<TOut>.FromTrusted(mapped)))
        );

    /// <summary>Removes duplicates while keeping the union branch.</summary>
    public static AllOrSome<T> DistinctValues<T>(this AllOrSome<T> source) =>
        source.Match(
            AllOrSome<T>.All,
            values => AllOrSome<T>.Some(NotEmptyList<T>.FromTrusted(values.Distinct()))
        );
}
