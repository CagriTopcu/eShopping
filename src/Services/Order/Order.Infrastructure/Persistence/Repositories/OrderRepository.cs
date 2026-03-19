using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions;

namespace Order.Infrastructure.Persistence.Repositories;

internal sealed class OrderRepository(OrderDbContext dbContext) : IOrderRepository
{
    public Task<Order.Domain.Entities.Order?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        dbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id.Value == id, ct);

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

    public async Task AddAsync(Order.Domain.Entities.Order order, CancellationToken ct = default) =>
        await dbContext.Orders.AddAsync(order, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        dbContext.SaveChangesAsync(ct);
}
