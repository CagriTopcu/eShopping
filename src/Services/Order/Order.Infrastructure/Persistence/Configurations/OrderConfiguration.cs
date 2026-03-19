using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Domain.Enums;
using Order.Domain.ValueObjects;

namespace Order.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order.Domain.Entities.Order>
{
    public void Configure(EntityTypeBuilder<Order.Domain.Entities.Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(id => id.Value, value => OrderId.From(value))
            .HasColumnName("id");

        builder.Property(o => o.CustomerId)
            .IsRequired()
            .HasColumnName("customer_id");

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion(s => (int)s, v => (OrderStatus)v)
            .HasColumnName("status");

        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("shipping_street").IsRequired().HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("shipping_city").IsRequired().HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("shipping_state").IsRequired().HasMaxLength(100);
            address.Property(a => a.Country).HasColumnName("shipping_country").IsRequired().HasMaxLength(100);
            address.Property(a => a.ZipCode).HasColumnName("shipping_zip_code").IsRequired().HasMaxLength(20);
        });

        builder.Property(o => o.PlacedAt)
            .IsRequired()
            .HasColumnName("placed_at");

        builder.Property(o => o.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(o => o.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnName("is_deleted");

        builder.Property(o => o.DeletedAt)
            .HasColumnName("deleted_at");

        builder.OwnsMany(o => o.Items, item =>
        {
            item.ToTable("order_items");

            item.HasKey(i => i.Id);

            item.Property(i => i.Id)
                .HasConversion(id => id.Value, value => new OrderItemId(value))
                .HasColumnName("id");

            item.Property(i => i.ProductId).IsRequired().HasColumnName("product_id");
            item.Property(i => i.ProductName).IsRequired().HasMaxLength(200).HasColumnName("product_name");
            item.Property(i => i.UnitPrice).IsRequired().HasPrecision(18, 2).HasColumnName("unit_price");
            item.Property(i => i.Quantity).IsRequired().HasColumnName("quantity");
            item.Ignore(i => i.LineTotal);

            item.WithOwner().HasForeignKey("order_id");
        });

        builder.HasIndex(o => o.CustomerId)
            .HasDatabaseName("ix_orders_customer_id");

        builder.HasIndex(o => o.Status)
            .HasDatabaseName("ix_orders_status");
    }
}
