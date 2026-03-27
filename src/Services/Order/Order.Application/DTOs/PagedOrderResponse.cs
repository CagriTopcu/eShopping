namespace Order.Application.DTOs;

public sealed record PagedOrderResponse(
    IReadOnlyList<OrderResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
