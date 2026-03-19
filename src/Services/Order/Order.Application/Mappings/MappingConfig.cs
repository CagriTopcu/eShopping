using Mapster;
using Order.Application.DTOs;
using Order.Domain.Entities;

namespace Order.Application.Mappings;

public static class MappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<OrderItem, OrderItemResponse>.NewConfig()
            .Map(dest => dest.ProductId, src => src.ProductId)
            .Map(dest => dest.ProductName, src => src.ProductName)
            .Map(dest => dest.UnitPrice, src => src.UnitPrice)
            .Map(dest => dest.Quantity, src => src.Quantity)
            .Map(dest => dest.LineTotal, src => src.LineTotal);

        TypeAdapterConfig<Order.Domain.Entities.Order, OrderResponse>.NewConfig()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.CustomerId, src => src.CustomerId)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.StatusName, src => src.Status.ToString())
            .Map(dest => dest.Items, src => src.Items.Adapt<List<OrderItemResponse>>())
            .Map(dest => dest.TotalAmount, src => src.TotalAmount)
            .Map(dest => dest.ShippingStreet, src => src.ShippingAddress.Street)
            .Map(dest => dest.ShippingCity, src => src.ShippingAddress.City)
            .Map(dest => dest.ShippingState, src => src.ShippingAddress.State)
            .Map(dest => dest.ShippingCountry, src => src.ShippingAddress.Country)
            .Map(dest => dest.ShippingZipCode, src => src.ShippingAddress.ZipCode)
            .Map(dest => dest.PlacedAt, src => src.PlacedAt);
    }
}
