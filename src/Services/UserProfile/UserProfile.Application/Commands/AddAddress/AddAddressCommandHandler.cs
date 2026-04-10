using Mapster;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;
using Shared.Contracts.Events.UserProfile;
using UserProfile.Application.Abstractions;
using UserProfile.Application.DTOs;
using UserProfile.Domain.Errors;
using UserProfile.Domain.ValueObjects;

namespace UserProfile.Application.Commands.AddAddress;

internal sealed class AddAddressCommandHandler(
    IProfileRepository repository,
    IPublishEndpoint publishEndpoint,
    ILogger<AddAddressCommandHandler> logger)
    : ICommandHandler<AddAddressCommand, AddressResponse>
{
    public async Task<Result<AddressResponse>> Handle(AddAddressCommand request, CancellationToken cancellationToken)
    {
        var profile = await repository.GetByKeycloakUserIdAsync(request.KeycloakUserId, cancellationToken);
        if (profile is null)
            return ProfileErrors.NotFound;

        var address = Address.Create(request.Street, request.City, request.State, request.ZipCode, request.Country);
        var result = profile.AddAddress(address, request.Label, request.IsDefault);
        if (result.IsFailure)
            return result.Error;

        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Address added for user {KeycloakUserId}", request.KeycloakUserId);

        await publishEndpoint.Publish(
            new UserAddressUpdatedEvent(profile.KeycloakUserId, request.City, request.Country),
            cancellationToken);

        return result.Value.Adapt<AddressResponse>();
    }
}
