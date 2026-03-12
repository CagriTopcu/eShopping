namespace Basket.API.Endpoints;

internal static class BasketEndpoints
{
    public static RouteGroupBuilder MapBasketEndpoints(this RouteGroupBuilder group)
    {
        GetBasketEndpoint.Map(group);
        UpsertBasketItemEndpoint.Map(group);
        RemoveBasketItemEndpoint.Map(group);
        DeleteBasketEndpoint.Map(group);

        return group;
    }
}
