using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Milese.Common.Shared;

/// <summary>
/// Represents either "all possible values" or a concrete, non-empty subset.
/// Replaces the implicit convention of "an empty list means all".
/// </summary>
/// <remarks>
/// Not a subtype-discriminated union: it's a single class with a discriminator, so
/// EF Core can map it as an owned type and, crucially, so IsAll and Items are
/// translatable inside a query. Invariants are preserved by construction: the
/// constructor is private, the only entry points are <see cref="All"/> and
/// <see cref="Some(NotEmptyList{T})"/>, and the collection is exposed read-only.
/// </remarks>
[JsonConverter(typeof(AllOrSomeJsonConverterFactory))]
public sealed class AllOrSome<T>
{
    private readonly List<T> items = [];
    private bool isAll;

    private AllOrSome() { }

    /// <summary>True when it represents the totality of possible values.</summary>
    /// <remarks>
    /// Prefer <see cref="Match"/> or <see cref="Switch"/>: reading this discriminator
    /// directly is how a branch gets silently forgotten, which is why it's banned by
    /// <c>BannedSymbols.txt</c> (RS0030).
    /// </remarks>
    public bool IsAll => isAll;

    /// <summary>
    /// The selected values. Always empty when IsAll is true, and always has
    /// at least one element otherwise.
    /// </summary>
    public IReadOnlyList<T> Items => items;

    /// <summary>All possible values.</summary>
    public static AllOrSome<T> All() => new() { isAll = true };

    /// <summary>A concrete subset, necessarily non-empty.</summary>
    public static AllOrSome<T> Some(NotEmptyList<T> values)
    {
        var result = new AllOrSome<T> { isAll = false };
        result.items.AddRange(values);
        return result;
    }

    /// <summary>
    /// Builds from any sequence: null or empty is interpreted as <see cref="All"/>.
    /// </summary>
    public static AllOrSome<T> FromValuesOrAll(IEnumerable<T>? values)
    {
        if (values is null)
            return All();

        var list = values.ToList();
        return list.Count == 0 ? All() : Some(NotEmptyList<T>.FromTrusted(list));
    }

    /// <summary>Forces both branches to be handled explicitly.</summary>
    public TOut Match<TOut>(Func<TOut> onAll, Func<IReadOnlyList<T>, TOut> onSome) =>
        isAll ? onAll() : onSome(items);

    /// <summary>Forces both branches to be handled explicitly.</summary>
    public void Switch(Action onAll, Action<IReadOnlyList<T>> onSome)
    {
        if (isAll)
            onAll();
        else
            onSome(items);
    }

    /// <summary>True if the value is included: always true when it represents the totality.</summary>
    public bool Contains(T value) => isAll || items.Contains(value);
}
