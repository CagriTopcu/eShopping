using MediatR;
using Shared.BuildingBlocks.Extensions;
using UserProfile.Application.Commands.RemoveAddress;

namespace UserProfile.API.Endpoints;

internal static class RemoveAddressEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("/me/addresses/{addressId:guid}", Handle)
            .WithName("RemoveAddress")
            .WithSummary("Remove address from profile")
            .WithDescription("Removes an address from the authenticated user's profile.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(404);

    private static async Task<IResult> Handle(
        Guid addressId,
        HttpContext httpContext,
        ISender sender,
        CancellationToken ct)
    {
        var userId = Guid.Parse(httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Missing sub claim."));

        var result = await sender.Send(new RemoveAddressCommand(userId, addressId), ct);
        return result.IsSuccess
            ? Results.NoContent()
            : result.ToHttpResult();
    }
}
