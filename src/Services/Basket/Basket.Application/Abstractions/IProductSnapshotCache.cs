using Basket.Application.DTOs;

namespace Basket.Application.Abstractions;

/// <summary>
/// Redis-backed local cache of product snapshots populated by integration events.
/// Eliminates sync HTTP calls to Catalog/Stock services during add-to-cart.
/// </summary>
public interface IProductSnapshotCache
{
    Task<ProductSnapshot?> GetAsync(Guid productId, CancellationToken ct = default);
    Task SetAsync(ProductSnapshot snapshot, CancellationToken ct = default);
    Task RemoveAsync(Guid productId, CancellationToken ct = default);
}
