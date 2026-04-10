using Asp.Versioning;
using Catalog.Application.Commands.CreateProduct;
using Catalog.Application.DTOs;
using MediatR;
using Shared.BuildingBlocks.Extensions;

namespace Catalog.API.Endpoints;

internal static class CreateProductEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("/products", Handle)
            .WithName("CreateProduct")
            .WithSummary("Create a new product")
            .WithDescription("Creates a new product in the catalog. Requires Admin role.")
            .Produces<Guid>(201)
            .ProducesProblem(400)
            .ProducesProblem(422)
            .RequireAuthorization("RequireAdmin");

    private static async Task<IResult> Handle(
        CreateProductRequest request,
        ISender sender,
        HttpContext context,
        CancellationToken ct)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Category,
            request.Price,
            request.Currency,
            request.InitialStock,
            request.Description,
            request.ImageUrl);

        var result = await sender.Send(command, ct);

        if (result.IsFailure)
            return result.ToHttpResult();

        var version = context.GetRequestedApiVersion();
        return Results.Created($"/api/v{version}/catalog/products/{result.Value}", result.Value);
    }
}
