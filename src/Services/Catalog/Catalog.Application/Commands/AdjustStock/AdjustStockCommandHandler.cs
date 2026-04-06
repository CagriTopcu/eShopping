using Catalog.Application.Abstractions;
using Catalog.Application.ReadModels;
using Catalog.Domain.Errors;
using Catalog.Domain.ValueObjects;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;

namespace Catalog.Application.Commands.AdjustStock;

internal sealed class AdjustStockCommandHandler(
    IProductWriteRepository writeRepository,
    IProductReadRepository readRepository,
    IPublisher publisher,
    ILogger<AdjustStockCommandHandler> logger)
    : ICommandHandler<AdjustStockCommand>
{
    public async Task<Result> Handle(
        AdjustStockCommand request,
        CancellationToken cancellationToken)
    {
        var product = await writeRepository.GetByIdAsync(
            ProductId.From(request.ProductId), cancellationToken);

        if (product is null)
            return ProductErrors.NotFound;

        var adjustResult = product.AdjustStock(request.Delta);
        if (adjustResult.IsFailure)
            return adjustResult.Error;

        await writeRepository.UpdateAsync(product, cancellationToken);
        await readRepository.UpsertAsync(product.Adapt<ProductReadModel>(), cancellationToken);

        foreach (var domainEvent in product.DomainEvents)
            await publisher.Publish(domainEvent, cancellationToken);

        product.ClearDomainEvents();

        logger.LogInformation(
            "Product {ProductId} stock adjusted by {Delta}, new stock {Stock}",
            product.Id.Value, request.Delta, product.Stock.Value);

        return Result.Success();
    }
}
