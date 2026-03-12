using Basket.Application.Abstractions;
using Basket.Application.DTOs;
using Basket.Domain.Errors;
using Mapster;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;

namespace Basket.Application.Queries.GetBasket;

internal sealed class GetBasketQueryHandler(IBasketRepository basketRepository)
    : IQueryHandler<GetBasketQuery, BasketResponse>
{
    public async Task<Result<BasketResponse>> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetAsync(request.Username, cancellationToken);

        if (basket is null)
            return BasketErrors.NotFound;

        return basket.Adapt<BasketResponse>();
    }
}
