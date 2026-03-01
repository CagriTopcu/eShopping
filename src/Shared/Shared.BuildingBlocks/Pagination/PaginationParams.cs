namespace Shared.BuildingBlocks.Pagination;

public sealed class PaginationParams
{
    private const int MaxPageSize = 50;
    private const int DefaultPageSize = 10;

    public PaginationParams(int page = 1, int pageSize = DefaultPageSize)
    {
        Page = page < 1 ? 1 : page;
        PageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize < 1 ? 1 : pageSize;
    }

    public int Page { get; }
    public int PageSize { get; }
    public int Skip => (Page - 1) * PageSize;
}
