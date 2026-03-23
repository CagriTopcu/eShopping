using MediatR;
using Shared.BuildingBlocks.Extensions;
using Stock.Application.Commands.SetStock;
using Stock.Application.DTOs;

namespace Stock.API.Endpoints;

internal static class SetStockEndpoint
{
    // Note: this endpoint is unauthenticated — StockService is internal-only,
    // protected by network topology. Add mutual TLS or an API key for production hardening.
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("/{productId:guid}", Handle)
            .WithName("SetStock")
            .WithSummary("Set stock level")
            .WithDescription("Sets the absolute stock quantity for a product. Internal service endpoint protected by network topology.")
            .Produces<StockResponse>()
            .ProducesValidationProblem();

    private static async Task<IResult> Handle(
        Guid productId,
        SetStockRequest request,
        ISender sender,
        CancellationToken ct) =>
        (await sender.Send(new SetStockCommand(productId, request.AvailableQuantity), ct)).ToHttpResult();
}

/// <summary>Request payload for setting stock level.</summary>
/// <param name="AvailableQuantity">Absolute stock quantity to set. Must be zero or positive.</param>
internal sealed record SetStockRequest(int AvailableQuantity);
