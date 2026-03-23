using Catalog.Application.DTOs;
using Catalog.Application.Queries.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.BuildingBlocks.Extensions;
using Shared.BuildingBlocks.Pagination;

namespace Catalog.API.Endpoints;

internal static class GetProductsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/products", Handle)
            .WithName("GetProducts")
            .WithSummary("List products")
            .WithDescription("Returns a paginated list of products. Supports filtering by category, name, and price range.")
            .Produces<PagedList<ProductResponse>>()
            .AllowAnonymous();

    private static async Task<IResult> Handle(
        [FromQuery] string? category,
        [FromQuery] string? name,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        ISender sender = null!,
        CancellationToken ct = default)
    {
        var query = new GetProductsQuery(
            category,
            name,
            minPrice,
            maxPrice,
            new PaginationParams(page, pageSize));

        return (await sender.Send(query, ct)).ToHttpResult();
    }
}
