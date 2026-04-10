using Mapster;
using Shipping.Application.DTOs;
using Shipping.Domain.Entities;

namespace Shipping.Application.Mappings;

public static class MappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Shipment, ShipmentResponse>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id.Value);
    }
}
