using Basket.Application.Abstractions;
using Basket.Application.DTOs;
using Basket.Domain.Errors;
using Mapster;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;

namespace Basket.Application.Queries.GetBasket;

internal sealed class GetBasketQueryHandler(
    IBasketRepository basketRepository,
    ILogger<GetBasketQueryHandler> logger)
    : IQueryHandler<GetBasketQuery, BasketResponse>
{
    public async Task<Result<BasketResponse>> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetAsync(request.Username, cancellationToken);

        if (basket is null)
        {
            logger.LogDebug("Basket not found for user {Username}", request.Username);
            return BasketErrors.NotFound;
        }

        return basket.Adapt<BasketResponse>();
    }
}
