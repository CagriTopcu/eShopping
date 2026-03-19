namespace Payment.API.Endpoints;

internal static class ReservePaymentEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("/reserve", Handle)
            .WithName("ReservePayment")
            .Produces<ReservePaymentResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status402PaymentRequired);

    private static IResult Handle(ReservePaymentRequest request)
    {
        if (request.Amount <= 0)
            return Results.Problem(
                title: "Invalid amount",
                detail: "Payment amount must be greater than zero.",
                statusCode: StatusCodes.Status402PaymentRequired);

        return Results.Ok(new ReservePaymentResponse(
            ReservationId: Guid.NewGuid(),
            OrderId: request.OrderId,
            Status: "Reserved",
            ReservedAt: DateTimeOffset.UtcNow));
    }
}

internal sealed record ReservePaymentRequest(
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string Currency);

internal sealed record ReservePaymentResponse(
    Guid ReservationId,
    Guid OrderId,
    string Status,
    DateTimeOffset ReservedAt);
