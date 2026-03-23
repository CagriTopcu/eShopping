using Catalog.Application.DTOs;
using Catalog.Application.Queries.GetProductById;
using MediatR;
using Shared.BuildingBlocks.Extensions;

namespace Catalog.API.Endpoints;

internal static class GetProductByIdEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/products/{id:guid}", Handle)
            .WithName("GetProductById")
            .WithSummary("Get product by ID")
            .WithDescription("Returns a single product by its unique identifier.")
            .Produces<ProductResponse>()
            .ProducesProblem(404)
            .AllowAnonymous();

    private static async Task<IResult> Handle(
        Guid id,
        ISender sender,
        CancellationToken ct)
    {
        return (await sender.Send(new GetProductByIdQuery(id), ct)).ToHttpResult();
    }
}
