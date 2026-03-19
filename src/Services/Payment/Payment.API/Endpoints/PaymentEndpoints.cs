namespace Payment.API.Endpoints;

internal static class PaymentEndpoints
{
    public static RouteGroupBuilder MapPaymentEndpoints(this RouteGroupBuilder group)
    {
        ReservePaymentEndpoint.Map(group);

        return group;
    }
}
