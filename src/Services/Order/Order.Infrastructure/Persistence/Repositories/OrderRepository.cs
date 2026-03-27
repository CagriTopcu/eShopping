using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions;
using Order.Application.DTOs;
using Order.Domain.Enums;
using Order.Domain.ValueObjects;

namespace Order.Infrastructure.Persistence.Repositories;

internal sealed class OrderRepository(OrderDbContext dbContext) : IOrderRepository
{
    public Task<Order.Domain.Entities.Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var orderId = OrderId.From(id);
        return dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);
    }

    public async Task<IReadOnlyList<Order.Domain.Entities.Order>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken ct = default)
    {
        return await dbContext.Orders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.PlacedAt)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<Order.Domain.Entities.Order> Items, int TotalCount)> GetAllPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = dbContext.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.PlacedAt);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<OrderStatsResponse> GetStatsAsync(CancellationToken ct = default)
    {
        var totalOrders = await dbContext.Orders.CountAsync(ct);
        var pendingOrders = await dbContext.Orders.CountAsync(o => o.Status == OrderStatus.Pending, ct);
        var confirmedOrders = await dbContext.Orders.CountAsync(o => o.Status == OrderStatus.Confirmed, ct);
        var cancelledOrders = await dbContext.Orders.CountAsync(o => o.Status == OrderStatus.Cancelled, ct);
        var totalRevenue = await dbContext.Orders
            .Where(o => o.Status == OrderStatus.Confirmed)
            .SelectMany(o => o.Items)
            .SumAsync(i => i.UnitPrice * i.Quantity, ct);

        return new OrderStatsResponse(totalOrders, pendingOrders, confirmedOrders, cancelledOrders, totalRevenue);
    }

    public async Task AddAsync(Order.Domain.Entities.Order order, CancellationToken ct = default) =>
        await dbContext.Orders.AddAsync(order, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        dbContext.SaveChangesAsync(ct);
}
