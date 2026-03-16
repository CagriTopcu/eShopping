namespace Stock.API.Endpoints;

internal static class StockEndpoints
{
    public static RouteGroupBuilder MapStockEndpoints(this RouteGroupBuilder group)
    {
        GetStockEndpoint.Map(group);
        SetStockEndpoint.Map(group);

        return group;
    }
}
