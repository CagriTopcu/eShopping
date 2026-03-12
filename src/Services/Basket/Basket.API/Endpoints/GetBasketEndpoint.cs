using Basket.Application.DTOs;
using Basket.Application.Queries.GetBasket;
using MediatR;
using Shared.BuildingBlocks.Extensions;

namespace Basket.API.Endpoints;

internal static class GetBasketEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/{username}", Handle)
            .WithName("GetBasket")
            .Produces<BasketResponse>()
            .ProducesProblem(404)
            .RequireAuthorization("RequireCustomer");

    private static async Task<IResult> Handle(
        string username,
        ISender sender,
        CancellationToken ct) =>
        (await sender.Send(new GetBasketQuery(username), ct)).ToHttpResult();
}
