using Basket.Application.DTOs;

namespace Basket.Application.Abstractions;

public interface IStockClient
{
    Task<StockInfo?> GetStockAsync(Guid productId, CancellationToken ct = default);
}
