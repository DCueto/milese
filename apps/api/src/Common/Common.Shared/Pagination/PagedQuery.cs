using System;
using System.Collections.Generic;

namespace Milese.Common.Shared.Pagination;

public enum SortDirection
{
    Asc = 1,
    Desc = 2,
}

public enum LoadMode
{
    All = 1,
    OnlyData = 2,
    OnlyCount = 3,
}

public sealed class SortColumn<TField>
    where TField : struct, Enum
{
    public required TField Field { get; init; }

    public required SortDirection Direction { get; init; }
}

public sealed class PagedQuery<TFilter, TField>
    where TField : struct, Enum
{
    public required int Page { get; init; }

    public required int PageSize { get; init; }

    public required List<SortColumn<TField>> Sort { get; init; }

    public required TFilter Filter { get; init; }

    public LoadMode LoadMode { get; init; } = LoadMode.All;
}

public sealed class PagedItems<T>
{
    public required List<T> Items { get; init; }

    public required int TotalItems { get; init; }

    public required int Page { get; init; }

    public required int PageSize { get; init; }

    public int TotalPages => PageSize == 0 ? 0 : (TotalItems + PageSize - 1) / PageSize;
}
