using Microsoft.EntityFrameworkCore;
using Shipping.Application.Abstractions;
using Shipping.Domain.Entities;
using Shipping.Domain.Enums;
using Shipping.Domain.ValueObjects;

namespace Shipping.Infrastructure.Persistence;

internal sealed class ShipmentRepository(ShippingDbContext dbContext) : IShipmentRepository
{
    public async Task<Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default) =>
        await dbContext.Shipments.FirstOrDefaultAsync(s => s.OrderId == orderId, ct);

    public async Task<Shipment?> GetByIdAsync(Guid shipmentId, CancellationToken ct = default) =>
        await dbContext.Shipments.FirstOrDefaultAsync(s => s.Id == new ShipmentId(shipmentId), ct);

    public async Task<(IReadOnlyList<Shipment> Items, int TotalCount)> GetPagedAsync(
        ShipmentStatus? status, int skip, int take, CancellationToken ct = default)
    {
        var query = dbContext.Shipments.AsQueryable();

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task AddAsync(Shipment shipment, CancellationToken ct = default) =>
        await dbContext.Shipments.AddAsync(shipment, ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await dbContext.SaveChangesAsync(ct);
}
