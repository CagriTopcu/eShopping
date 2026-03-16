using Microsoft.EntityFrameworkCore;
using Stock.Application.Abstractions;
using Stock.Domain.Entities;

namespace Stock.Infrastructure.Persistence;

internal sealed class StockRepository(StockDbContext dbContext) : IStockRepository
{
    public Task<StockItem?> GetByProductIdAsync(Guid productId, CancellationToken ct = default) =>
        dbContext.StockItems.FirstOrDefaultAsync(s => s.ProductId == productId, ct);

    public async Task AddAsync(StockItem stockItem, CancellationToken ct = default) =>
        await dbContext.StockItems.AddAsync(stockItem, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        dbContext.SaveChangesAsync(ct);
}
