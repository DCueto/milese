using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Milese.Common.Shared.Pagination;

public sealed class SortableField<TDb, TField>
    where TField : struct, Enum
{
    public required TField Field { get; init; }

    public required Expression<Func<TDb, object>> KeySelector { get; init; }
}

public static class SortableFieldExtensions
{
    public static IQueryable<TDb> ApplySorting<TDb, TField>(
        this IQueryable<TDb> query,
        List<SortColumn<TField>> sorts,
        NotEmptyList<SortableField<TDb, TField>> sortableFields
    )
        where TField : struct, Enum
    {
        IOrderedQueryable<TDb>? ordered = null;

        foreach (var sort in sorts)
        {
            var field = sortableFields.FirstOrDefault(f => f.Field.Equals(sort.Field));
            if (field is null)
                continue;

            ordered = (ordered, sort.Direction) switch
            {
                (null, SortDirection.Asc) => query.OrderBy(field.KeySelector),
                (null, SortDirection.Desc) => query.OrderByDescending(field.KeySelector),
                (not null, SortDirection.Asc) => ordered.ThenBy(field.KeySelector),
                (not null, SortDirection.Desc) => ordered.ThenByDescending(field.KeySelector),
            };
        }

        return ordered ?? query.OrderBy(sortableFields[0].KeySelector);
    }
}
