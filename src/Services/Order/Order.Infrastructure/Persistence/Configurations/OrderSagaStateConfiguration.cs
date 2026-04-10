using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Order.Application.Sagas;

namespace Order.Infrastructure.Persistence.Configurations;

internal sealed class OrderSagaStateConfiguration : IEntityTypeConfiguration<OrderSagaState>
{
    public void Configure(EntityTypeBuilder<OrderSagaState> builder)
    {
        builder.HasKey(s => s.CorrelationId);

        builder.Property(s => s.CurrentState)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(s => s.ItemsJson)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(s => s.TransactionId)
            .HasMaxLength(128);

        builder.ToTable("order_saga_states");
    }
}
