namespace UserProfile.API.Endpoints;

internal static class ProfileEndpoints
{
    public static RouteGroupBuilder MapProfileEndpoints(this RouteGroupBuilder group)
    {
        GetProfileEndpoint.Map(group);
        CreateProfileEndpoint.Map(group);
        UpdateProfileEndpoint.Map(group);
        AddAddressEndpoint.Map(group);
        RemoveAddressEndpoint.Map(group);

        return group;
    }
}
