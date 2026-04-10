using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipping.Domain.Entities;
using Shipping.Domain.ValueObjects;

namespace Shipping.Infrastructure.Persistence.Configurations;

internal sealed class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(
                id => id.Value,
                value => new ShipmentId(value));

        builder.Property(s => s.OrderId).IsRequired();
        builder.HasIndex(s => s.OrderId).IsUnique();

        builder.Property(s => s.CustomerId).IsRequired();
        builder.Property(s => s.OrderTotal).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.Status).IsRequired().HasConversion<string>();
        builder.Property(s => s.TrackingNumber).HasMaxLength(100);
        builder.Property(s => s.CreatedAt).IsRequired();
    }
}
