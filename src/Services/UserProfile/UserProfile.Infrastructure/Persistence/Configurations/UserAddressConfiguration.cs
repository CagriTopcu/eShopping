using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserProfile.Domain.Entities;
using UserProfile.Domain.ValueObjects;

namespace UserProfile.Infrastructure.Persistence.Configurations;

internal sealed class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion(
                id => id.Value,
                value => new AddressId(value));

        builder.OwnsOne(a => a.Address, address =>
        {
            address.Property(v => v.Street).IsRequired().HasMaxLength(200).HasColumnName("Street");
            address.Property(v => v.City).IsRequired().HasMaxLength(100).HasColumnName("City");
            address.Property(v => v.State).IsRequired().HasMaxLength(100).HasColumnName("State");
            address.Property(v => v.ZipCode).IsRequired().HasMaxLength(20).HasColumnName("ZipCode");
            address.Property(v => v.Country).IsRequired().HasMaxLength(100).HasColumnName("Country");
        });

        builder.Property(a => a.Label).IsRequired().HasMaxLength(50);
        builder.Property(a => a.IsDefault).IsRequired();
    }
}
