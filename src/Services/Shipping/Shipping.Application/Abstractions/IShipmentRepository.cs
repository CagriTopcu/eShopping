using Shipping.Domain.Entities;
using Shipping.Domain.Enums;

namespace Shipping.Application.Abstractions;

public interface IShipmentRepository
{
    Task<Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task<Shipment?> GetByIdAsync(Guid shipmentId, CancellationToken ct = default);
    Task<(IReadOnlyList<Shipment> Items, int TotalCount)> GetPagedAsync(
        ShipmentStatus? status, int skip, int take, CancellationToken ct = default);
    Task AddAsync(Shipment shipment, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
