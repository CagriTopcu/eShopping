using Stock.Domain.Entities;

namespace Stock.Application.Abstractions;

public interface IStockRepository
{
    Task<StockItem?> GetByProductIdAsync(Guid productId, CancellationToken ct = default);
    Task AddAsync(StockItem stockItem, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
