namespace Shared.BuildingBlocks.Pagination;

public sealed class PagedList<T>
{
    private PagedList(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
    public bool IsEmpty => TotalCount == 0;

    public static PagedList<T> Create(IEnumerable<T> source, int page, int pageSize)
    {
        var totalCount = source.Count();
        var items = source.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return new PagedList<T>(items.AsReadOnly(), page, pageSize, totalCount);
    }

    public static PagedList<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalCount) =>
        new(items, page, pageSize, totalCount);

    public PagedList<TNew> Map<TNew>(Func<T, TNew> mapper) =>
        new(Items.Select(mapper).ToList().AsReadOnly(), Page, PageSize, TotalCount);
}
