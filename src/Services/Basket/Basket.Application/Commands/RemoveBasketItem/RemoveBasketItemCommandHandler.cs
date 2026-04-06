using Basket.Application.Abstractions;
using Basket.Domain.Errors;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;

namespace Basket.Application.Commands.RemoveBasketItem;

internal sealed class RemoveBasketItemCommandHandler(
    IBasketRepository basketRepository,
    ILogger<RemoveBasketItemCommandHandler> logger)
    : ICommandHandler<RemoveBasketItemCommand>
{
    public async Task<Result> Handle(RemoveBasketItemCommand request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetAsync(request.Username, cancellationToken);

        if (basket is null)
            return BasketErrors.NotFound;

        var result = basket.RemoveItem(request.ProductId);

        if (result.IsFailure)
            return result;

        await basketRepository.SaveAsync(basket, cancellationToken);

        logger.LogInformation(
            "Basket item removed for user {Username}: product {ProductId}",
            request.Username, request.ProductId);

        return Result.Success();
    }
}
