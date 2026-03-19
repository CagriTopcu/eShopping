using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.Application.Sagas;

namespace Order.Infrastructure.Persistence;

public sealed class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Order.Domain.Entities.Order> Orders => Set<Order.Domain.Entities.Order>();
    public DbSet<OrderSagaState> OrderSagaStates => Set<OrderSagaState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);

        modelBuilder.Entity<Order.Domain.Entities.Order>()
            .HasQueryFilter(o => !o.IsDeleted);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
    }
}
