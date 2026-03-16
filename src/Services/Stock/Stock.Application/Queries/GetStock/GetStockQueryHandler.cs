using Mapster;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;
using Stock.Application.Abstractions;
using Stock.Application.DTOs;
using Stock.Domain.Errors;

namespace Stock.Application.Queries.GetStock;

internal sealed class GetStockQueryHandler(IStockRepository repository)
    : IQueryHandler<GetStockQuery, StockResponse>
{
    public async Task<Result<StockResponse>> Handle(GetStockQuery request, CancellationToken cancellationToken)
    {
        var stockItem = await repository.GetByProductIdAsync(request.ProductId, cancellationToken);

        if (stockItem is null)
            return StockErrors.NotFound;

        return stockItem.Adapt<StockResponse>();
    }
}
