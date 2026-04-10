using Mapster;
using UserProfile.Application.DTOs;
using UserProfile.Domain.Entities;

namespace UserProfile.Application.Mappings;

public static class MappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Profile, ProfileResponse>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Addresses, src => src.Addresses);

        TypeAdapterConfig<UserAddress, AddressResponse>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Street, src => src.Address.Street)
            .Map(dest => dest.City, src => src.Address.City)
            .Map(dest => dest.State, src => src.Address.State)
            .Map(dest => dest.ZipCode, src => src.Address.ZipCode)
            .Map(dest => dest.Country, src => src.Address.Country);
    }
}
