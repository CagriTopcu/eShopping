using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserProfile.Domain.Entities;
using UserProfile.Domain.ValueObjects;

namespace UserProfile.Infrastructure.Persistence.Configurations;

internal sealed class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => new UserProfileId(value));

        builder.Property(p => p.KeycloakUserId).IsRequired();
        builder.HasIndex(p => p.KeycloakUserId).IsUnique();

        builder.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.LastName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Email).IsRequired().HasMaxLength(256);
        builder.Property(p => p.PhoneNumber).HasMaxLength(20);

        builder.HasMany(p => p.Addresses)
            .WithOne()
            .HasForeignKey("ProfileId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Addresses).AutoInclude();
    }
}
