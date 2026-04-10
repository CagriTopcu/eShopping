namespace UserProfile.Domain.ValueObjects;

public sealed record UserProfileId(Guid Value)
{
    public static UserProfileId New() => new(Guid.NewGuid());
}
