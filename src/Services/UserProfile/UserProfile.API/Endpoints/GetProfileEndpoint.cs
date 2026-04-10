using MediatR;
using Shared.BuildingBlocks.Extensions;
using UserProfile.Application.DTOs;
using UserProfile.Application.Queries.GetProfile;

namespace UserProfile.API.Endpoints;

internal static class GetProfileEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("/me", Handle)
            .WithName("GetProfile")
            .WithSummary("Get current user profile")
            .WithDescription("Returns the profile and addresses for the authenticated user.")
            .Produces<ProfileResponse>()
            .ProducesProblem(404);

    private static async Task<IResult> Handle(
        HttpContext httpContext,
        ISender sender,
        CancellationToken ct)
    {
        var userId = GetKeycloakUserId(httpContext);
        return (await sender.Send(new GetProfileQuery(userId), ct)).ToHttpResult();
    }

    private static Guid GetKeycloakUserId(HttpContext context) =>
        Guid.Parse(context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Missing sub claim."));
}
