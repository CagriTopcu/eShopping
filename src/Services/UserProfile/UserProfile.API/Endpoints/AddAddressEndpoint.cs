using MediatR;
using Shared.BuildingBlocks.Extensions;
using UserProfile.Application.Commands.AddAddress;
using UserProfile.Application.DTOs;

namespace UserProfile.API.Endpoints;

internal static class AddAddressEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("/me/addresses", Handle)
            .WithName("AddAddress")
            .WithSummary("Add address to profile")
            .WithDescription("Adds a new address to the authenticated user's profile.")
            .Produces<AddressResponse>(StatusCodes.Status201Created)
            .ProducesProblem(404)
            .ProducesValidationProblem();

    private static async Task<IResult> Handle(
        AddAddressRequest request,
        HttpContext httpContext,
        ISender sender,
        CancellationToken ct)
    {
        var userId = Guid.Parse(httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Missing sub claim."));

        var command = new AddAddressCommand(
            userId,
            request.Street,
            request.City,
            request.State,
            request.ZipCode,
            request.Country,
            request.Label,
            request.IsDefault);

        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? Results.Created($"/api/v1/profile/me", result.Value)
            : result.ToHttpResult();
    }
}

internal sealed record AddAddressRequest(
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country,
    string Label,
    bool IsDefault = false);
