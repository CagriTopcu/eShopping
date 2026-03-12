using Basket.Application.Abstractions;
using Basket.Domain.Errors;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;

namespace Basket.Application.Commands.RemoveBasketItem;

internal sealed class RemoveBasketItemCommandHandler(IBasketRepository basketRepository)
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

        return Result.Success();
    }
}
