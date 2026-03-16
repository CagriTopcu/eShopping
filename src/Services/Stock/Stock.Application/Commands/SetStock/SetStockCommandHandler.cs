using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;
using Stock.Application.Abstractions;
using Stock.Application.DTOs;
using Stock.Domain.Entities;

namespace Stock.Application.Commands.SetStock;

internal sealed class SetStockCommandHandler(IStockRepository repository)
    : ICommandHandler<SetStockCommand, StockResponse>
{
    public async Task<Result<StockResponse>> Handle(SetStockCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetByProductIdAsync(request.ProductId, cancellationToken);

        if (existing is null)
        {
            var item = StockItem.Create(request.ProductId, request.AvailableQuantity);
            await repository.AddAsync(item, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
            return new StockResponse(item.ProductId, item.AvailableQuantity);
        }

        var result = existing.SetAvailableQuantity(request.AvailableQuantity);
        if (result.IsFailure)
            return result.Error;

        await repository.SaveChangesAsync(cancellationToken);
        return new StockResponse(existing.ProductId, existing.AvailableQuantity);
    }
}
