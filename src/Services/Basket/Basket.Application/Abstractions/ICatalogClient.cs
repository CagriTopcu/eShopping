using Basket.Application.DTOs;

namespace Basket.Application.Abstractions;

public interface ICatalogClient
{
    Task<ProductSnapshot?> GetProductSnapshotAsync(Guid productId, CancellationToken ct = default);
}
