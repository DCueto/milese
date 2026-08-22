using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Milese.Common.Shared;
using Milese.Common.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Milese.Common.Server;

public static class QueryablePaginationExtensions
{
    public static async Task<PagedItems<TBo>> ToPagedResultAsync<TDb, TBo, TFilter, TField>(
        this IQueryable<TDb> query,
        PagedQuery<TFilter, TField> pagedQuery,
        NotEmptyList<SortableField<TDb, TField>> sortableFields,
        Expression<Func<TDb, TBo>> selector,
        CancellationToken cancellationToken
    )
        where TField : struct, Enum
    {
        var totalItems = 0;
        if (pagedQuery.LoadMode != LoadMode.OnlyData)
            totalItems = await query.CountAsync(cancellationToken);

        var items = new List<TBo>();
        if (pagedQuery.LoadMode != LoadMode.OnlyCount)
        {
            items = await query
                .ApplySorting(pagedQuery.Sort, sortableFields)
                .Skip((pagedQuery.Page - 1) * pagedQuery.PageSize)
                .Take(pagedQuery.PageSize)
                .Select(selector)
                .ToListAsync(cancellationToken);
        }

        return new PagedItems<TBo>
        {
            Items = items,
            TotalItems = totalItems,
            Page = pagedQuery.Page,
            PageSize = pagedQuery.PageSize,
        };
    }

    /// <summary>
    /// Variant that materializes the entities and applies the mapping in memory. Needed when the
    /// mapping is polymorphic and cannot be translated into a SQL projection.
    /// </summary>
    public static async Task<PagedItems<TBo>> ToPagedResultMappedAsync<TDb, TBo, TFilter, TField>(
        this IQueryable<TDb> query,
        PagedQuery<TFilter, TField> pagedQuery,
        NotEmptyList<SortableField<TDb, TField>> sortableFields,
        Func<TDb, TBo> mapper,
        CancellationToken cancellationToken
    )
        where TField : struct, Enum
    {
        var totalItems = 0;
        if (pagedQuery.LoadMode != LoadMode.OnlyData)
            totalItems = await query.CountAsync(cancellationToken);

        var items = new List<TBo>();
        if (pagedQuery.LoadMode != LoadMode.OnlyCount)
        {
            var dbItems = await query
                .ApplySorting(pagedQuery.Sort, sortableFields)
                .Skip((pagedQuery.Page - 1) * pagedQuery.PageSize)
                .Take(pagedQuery.PageSize)
                .ToListAsync(cancellationToken);

            items = dbItems.ConvertAll(db => mapper(db));
        }

        return new PagedItems<TBo>
        {
            Items = items,
            TotalItems = totalItems,
            Page = pagedQuery.Page,
            PageSize = pagedQuery.PageSize,
        };
    }
}
