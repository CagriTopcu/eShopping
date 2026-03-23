using Catalog.Application.Commands.UpdateProduct;
using Catalog.Application.DTOs;
using MediatR;
using Shared.BuildingBlocks.Extensions;

namespace Catalog.API.Endpoints;

internal static class UpdateProductEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("/products/{id:guid}", Handle)
            .WithName("UpdateProduct")
            .WithSummary("Update a product")
            .WithDescription("Updates an existing product's details. Requires Admin role.")
            .Produces(204)
            .ProducesProblem(404)
            .ProducesProblem(422)
            .RequireAuthorization("RequireAdmin");

    private static async Task<IResult> Handle(
        Guid id,
        UpdateProductRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Category,
            request.Price,
            request.Currency,
            request.Description,
            request.ImageUrl);

        var result = await sender.Send(command, ct);

        if (result.IsFailure)
            return result.ToHttpResult();

        return Results.NoContent();
    }
}
