using Shared.BuildingBlocks.Results;

namespace UserProfile.Domain.Errors;

public static class ProfileErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Profile.NotFound", "User profile was not found.");

    public static readonly Error AlreadyExists =
        Error.Conflict("Profile.AlreadyExists", "A profile already exists for this user.");

    public static readonly Error AddressNotFound =
        Error.NotFound("Profile.AddressNotFound", "Address was not found.");

    public static readonly Error MaxAddressesReached =
        Error.Validation("Profile.MaxAddressesReached", "Maximum of 10 addresses per profile.");
}
