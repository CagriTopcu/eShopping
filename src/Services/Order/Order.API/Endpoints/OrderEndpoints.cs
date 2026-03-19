namespace Order.API.Endpoints;

internal static class OrderEndpoints
{
    public static RouteGroupBuilder MapOrderEndpoints(this RouteGroupBuilder group)
    {
        PlaceOrderEndpoint.Map(group);
        GetOrdersByUserEndpoint.Map(group);
        GetOrderByIdEndpoint.Map(group);
        CancelOrderEndpoint.Map(group);

        return group;
    }
}
