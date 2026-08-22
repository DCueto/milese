using System.Collections.Generic;
using System.Linq;

namespace Milese.Common.Shared;

/// <summary>
/// A list guaranteed to hold at least one element. Cannot be constructed empty.
/// </summary>
public class NotEmptyList<T> : List<T>
{
    public NotEmptyList(T firstElement) => Add(firstElement);

    private NotEmptyList(IEnumerable<T> elements) : base(elements) { }

    /// <summary>
    /// Trusted construction: the caller guarantees the sequence is not empty.
    /// </summary>
    public static NotEmptyList<T> FromTrusted(IEnumerable<T> elements) => new(elements);

    /// <summary>
    /// Construction from untrusted data: fails if the sequence is null or empty.
    /// </summary>
    public static Result<NotEmptyList<T>, InvalidData> Parse(IEnumerable<T>? elements)
    {
        if (elements is null)
            return NotEmptyError(new InvalidDataConstraint.CanNotBeNull());

        var list = elements.ToList();
        return list.Count == 0
            ? NotEmptyError(new InvalidDataConstraint.CanNotBeEmptyOrNull())
            : new NotEmptyList<T>(list);
    }

    private static InvalidData NotEmptyError(InvalidDataConstraint constraint) =>
        new()
        {
            Constraint = constraint,
            FieldName = typeof(T).Name,
        };
}
