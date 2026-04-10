using Catalog.Application.DTOs;
using Catalog.Application.ReadModels;
using Catalog.Domain.Entities;
using Mapster;

namespace Catalog.Application.Mappings;

public static class MappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Product, ProductReadModel>.NewConfig()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Name, src => src.Name.Value)
            .Map(dest => dest.Category, src => src.Category.Name)
            .Map(dest => dest.Price, src => src.Price.Amount)
            .Map(dest => dest.Currency, src => src.Price.Currency);

        TypeAdapterConfig<Product, ProductResponse>.NewConfig()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Name, src => src.Name.Value)
            .Map(dest => dest.Category, src => src.Category.Name)
            .Map(dest => dest.Price, src => src.Price.Amount)
            .Map(dest => dest.Currency, src => src.Price.Currency);

        TypeAdapterConfig<ProductReadModel, ProductResponse>.NewConfig();
    }
}
