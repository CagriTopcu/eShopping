using MediatR;
using Shared.BuildingBlocks.Extensions;
using Stock.Application.Queries.GetStock;

namespace Stock.API.Endpoints;

internal static class GetStockEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/{productId:guid}", Handle)
            .WithName("GetStock")
            .Produces<Stock.Application.DTOs.StockResponse>()
            .ProducesProblem(404);

    private static async Task<IResult> Handle(
        Guid productId,
        ISender sender,
        CancellationToken ct) =>
        (await sender.Send(new GetStockQuery(productId), ct)).ToHttpResult();
}
