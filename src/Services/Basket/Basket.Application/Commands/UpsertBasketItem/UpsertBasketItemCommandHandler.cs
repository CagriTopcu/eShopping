using Basket.Application.Abstractions;
using Basket.Application.DTOs;
using Basket.Domain.Errors;
using Mapster;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;

namespace Basket.Application.Commands.UpsertBasketItem;

internal sealed class UpsertBasketItemCommandHandler(
    IBasketRepository basketRepository,
    ICatalogClient catalogClient,
    IStockClient stockClient)
    : ICommandHandler<UpsertBasketItemCommand, BasketResponse>
{
    public async Task<Result<BasketResponse>> Handle(
        UpsertBasketItemCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Resolve product metadata from Catalog (name, price, currency).
        ProductSnapshot? snapshot;
        try
        {
            snapshot = await catalogClient.GetProductSnapshotAsync(request.ProductId, cancellationToken);
        }
        catch (CatalogServiceUnavailableException)
        {
            return BasketErrors.CatalogUnavailable;
        }

        if (snapshot is null)
            return BasketErrors.ProductNotFound;

        // 2. Query stock availability from StockService (soft check).
        //    null means no stock record exists → treated as unconstrained (permissive).
        //    Hard reservation happens in the Order saga at checkout.
        StockInfo? stockInfo;
        try
        {
            stockInfo = await stockClient.GetStockAsync(request.ProductId, cancellationToken);
        }
        catch (StockServiceUnavailableException)
        {
            return BasketErrors.StockUnavailable;
        }

        var basket = await basketRepository.GetAsync(request.Username, cancellationToken)
            ?? Basket.Domain.Entities.Basket.Create(request.Username);

        var result = basket.AddOrUpdateItem(
            snapshot.ProductId,
            snapshot.Name,
            snapshot.Price,
            snapshot.Currency,
            request.Quantity,
            stockInfo?.AvailableQuantity);

        if (result.IsFailure)
            return result.Error;

        await basketRepository.SaveAsync(basket, cancellationToken);

        return basket.Adapt<BasketResponse>();
    }
}
