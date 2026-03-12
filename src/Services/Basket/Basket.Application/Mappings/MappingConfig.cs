using Basket.Application.DTOs;
using Basket.Domain.Entities;
using Mapster;

namespace Basket.Application.Mappings;

public static class MappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Basket.Domain.Entities.Basket, BasketResponse>.NewConfig()
            .Map(dest => dest.Username, src => src.Id.Value)
            .Map(dest => dest.Items, src => src.Items.Adapt<IReadOnlyList<BasketItemResponse>>())
            .Map(dest => dest.TotalPrice, src => src.TotalPrice);

        TypeAdapterConfig<BasketItem, BasketItemResponse>.NewConfig()
            .Map(dest => dest.LineTotal, src => src.LineTotal);
    }
}
