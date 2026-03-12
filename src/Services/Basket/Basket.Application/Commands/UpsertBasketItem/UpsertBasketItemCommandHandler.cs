using Basket.Application.Abstractions;
using Basket.Application.DTOs;
using Basket.Domain.Errors;
using Mapster;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;

namespace Basket.Application.Commands.UpsertBasketItem;

internal sealed class UpsertBasketItemCommandHandler(
    IBasketRepository basketRepository,
    ICatalogClient catalogClient)
    : ICommandHandler<UpsertBasketItemCommand, BasketResponse>
{
    public async Task<Result<BasketResponse>> Handle(
        UpsertBasketItemCommand request,
        CancellationToken cancellationToken)
    {
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

        var basket = await basketRepository.GetAsync(request.Username, cancellationToken)
            ?? Basket.Domain.Entities.Basket.Create(request.Username);

        var result = basket.AddOrUpdateItem(
            snapshot.ProductId,
            snapshot.Name,
            snapshot.Price,
            snapshot.Currency,
            request.Quantity,
            snapshot.Stock);

        if (result.IsFailure)
            return result.Error;

        await basketRepository.SaveAsync(basket, cancellationToken);

        return basket.Adapt<BasketResponse>();
    }
}
