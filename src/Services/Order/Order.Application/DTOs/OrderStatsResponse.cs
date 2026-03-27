namespace Order.Application.DTOs;

public sealed record OrderStatsResponse(
    int TotalOrders,
    int PendingOrders,
    int ConfirmedOrders,
    int CancelledOrders,
    decimal TotalRevenue);
