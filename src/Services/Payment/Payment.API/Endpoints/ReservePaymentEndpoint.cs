namespace Payment.API.Endpoints;

internal static class ReservePaymentEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("/reserve", Handle)
            .WithName("ReservePayment")
            .WithSummary("Reserve payment")
            .WithDescription("Reserves funds for an order. Internal service endpoint called during order saga orchestration.")
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

/// <summary>Request payload for reserving payment.</summary>
/// <param name="OrderId">Unique identifier of the order.</param>
/// <param name="CustomerId">Unique identifier of the customer.</param>
/// <param name="Amount">Payment amount to reserve. Must be greater than zero.</param>
/// <param name="Currency">ISO 4217 currency code (e.g., USD, EUR, TRY).</param>
internal sealed record ReservePaymentRequest(
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string Currency);

/// <summary>Response after a successful payment reservation.</summary>
/// <param name="ReservationId">Unique identifier of the payment reservation.</param>
/// <param name="OrderId">Associated order identifier.</param>
/// <param name="Status">Reservation status (e.g., Reserved).</param>
/// <param name="ReservedAt">Timestamp when the reservation was created (UTC).</param>
internal sealed record ReservePaymentResponse(
    Guid ReservationId,
    Guid OrderId,
    string Status,
    DateTimeOffset ReservedAt);
