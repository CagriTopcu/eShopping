using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;
using UserProfile.Application.Abstractions;
using UserProfile.Domain.Errors;
using UserProfile.Domain.ValueObjects;

namespace UserProfile.Application.Commands.RemoveAddress;

internal sealed class RemoveAddressCommandHandler(
    IProfileRepository repository,
    ILogger<RemoveAddressCommandHandler> logger)
    : ICommandHandler<RemoveAddressCommand>
{
    public async Task<Result> Handle(RemoveAddressCommand request, CancellationToken cancellationToken)
    {
        var profile = await repository.GetByKeycloakUserIdAsync(request.KeycloakUserId, cancellationToken);
        if (profile is null)
            return ProfileErrors.NotFound;

        var result = profile.RemoveAddress(new AddressId(request.AddressId));
        if (result.IsFailure)
            return result;

        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Address {AddressId} removed for user {KeycloakUserId}",
            request.AddressId, request.KeycloakUserId);

        return Result.Success();
    }
}
