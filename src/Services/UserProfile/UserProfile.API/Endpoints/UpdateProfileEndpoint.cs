using MediatR;
using Shared.BuildingBlocks.Extensions;
using UserProfile.Application.Commands.UpdateProfile;
using UserProfile.Application.DTOs;

namespace UserProfile.API.Endpoints;

internal static class UpdateProfileEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("/me", Handle)
            .WithName("UpdateProfile")
            .WithSummary("Update user profile")
            .WithDescription("Updates the profile for the authenticated user.")
            .Produces<ProfileResponse>()
            .ProducesProblem(404)
            .ProducesValidationProblem();

    private static async Task<IResult> Handle(
        UpdateProfileRequest request,
        HttpContext httpContext,
        ISender sender,
        CancellationToken ct)
    {
        var userId = Guid.Parse(httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Missing sub claim."));

        var command = new UpdateProfileCommand(
            userId,
            request.FirstName,
            request.LastName,
            request.PhoneNumber);

        return (await sender.Send(command, ct)).ToHttpResult();
    }
}

internal sealed record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber);
