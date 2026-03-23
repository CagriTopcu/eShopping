using Catalog.Application.Commands.DeleteProduct;
using MediatR;
using Shared.BuildingBlocks.Extensions;

namespace Catalog.API.Endpoints;

internal static class DeleteProductEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("/products/{id:guid}", Handle)
            .WithName("DeleteProduct")
            .WithSummary("Delete a product")
            .WithDescription("Permanently removes a product from the catalog. Fails if product has active orders (409 Conflict). Requires Admin role.")
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(409)
            .RequireAuthorization("RequireAdmin");

    private static async Task<IResult> Handle(
        Guid id,
        ISender sender,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeleteProductCommand(id), ct);

        if (result.IsFailure)
            return result.ToHttpResult();

        return Results.NoContent();
    }
}
