namespace Shipping.API.Endpoints;

internal static class ShippingEndpoints
{
    public static RouteGroupBuilder MapShippingEndpoints(this RouteGroupBuilder group)
    {
        GetShipmentEndpoint.Map(group);
        GetShipmentsEndpoint.Map(group);
        UpdateShipmentStatusEndpoint.Map(group);

        return group;
    }
}
