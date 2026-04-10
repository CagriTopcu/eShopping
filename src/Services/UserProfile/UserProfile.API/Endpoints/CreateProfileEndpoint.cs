using MediatR;
using Shared.BuildingBlocks.Extensions;
using UserProfile.Application.Commands.CreateProfile;
using UserProfile.Application.DTOs;

namespace UserProfile.API.Endpoints;

internal static class CreateProfileEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("/", Handle)
            .WithName("CreateProfile")
            .WithSummary("Create user profile")
            .WithDescription("Creates a new profile for the authenticated user.")
            .Produces<ProfileResponse>(StatusCodes.Status201Created)
            .ProducesProblem(409)
            .ProducesValidationProblem();

    private static async Task<IResult> Handle(
        CreateProfileRequest request,
        HttpContext httpContext,
        ISender sender,
        CancellationToken ct)
    {
        var userId = Guid.Parse(httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Missing sub claim."));

        var command = new CreateProfileCommand(
            userId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber);

        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? Results.Created($"/api/v1/profile/me", result.Value)
            : result.ToHttpResult();
    }
}

internal sealed record CreateProfileRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber);
