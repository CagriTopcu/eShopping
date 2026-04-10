using Basket.Application.Abstractions;
using Basket.Application.DTOs;
using Basket.Domain.Errors;
using Mapster;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;

namespace Basket.Application.Commands.UpsertBasketItem;

internal sealed class UpsertBasketItemCommandHandler(
    IBasketRepository basketRepository,
    IProductSnapshotCache productSnapshotCache,
    ILogger<UpsertBasketItemCommandHandler> logger)
    : ICommandHandler<UpsertBasketItemCommand, BasketResponse>
{
    public async Task<Result<BasketResponse>> Handle(
        UpsertBasketItemCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Resolve product metadata from event-driven Redis cache (no sync HTTP).
        var snapshot = await productSnapshotCache.GetAsync(request.ProductId, cancellationToken);

        if (snapshot is null)
            return BasketErrors.ProductNotFound;

        // 2. Stock check is fully optimistic — hard reservation happens in the Order saga.
        //    The cache contains product metadata only (name, price, currency).
        //    No sync call to Stock service required.

        var basket = await basketRepository.GetAsync(request.Username, cancellationToken)
            ?? Basket.Domain.Entities.Basket.Create(request.Username);

        var result = basket.AddOrUpdateItem(
            snapshot.ProductId,
            snapshot.Name,
            snapshot.Price,
            snapshot.Currency,
            request.Quantity,
            availableStock: null); // Optimistic: stock validated at checkout via saga

        if (result.IsFailure)
            return result.Error;

        await basketRepository.SaveAsync(basket, cancellationToken);

        logger.LogInformation(
            "Basket item upserted for user {Username}: product {ProductId}, quantity {Quantity}",
            request.Username, request.ProductId, request.Quantity);

        return basket.Adapt<BasketResponse>();
    }
}
