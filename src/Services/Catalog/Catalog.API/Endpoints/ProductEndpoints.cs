namespace Catalog.API.Endpoints;

internal static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this RouteGroupBuilder group)
    {
        GetProductsEndpoint.Map(group);
        GetProductByIdEndpoint.Map(group);
        CreateProductEndpoint.Map(group);
        UpdateProductEndpoint.Map(group);
        DeleteProductEndpoint.Map(group);
        GetCatalogStatsEndpoint.Map(group);

        return group;
    }
}
