namespace Order.Application.Abstractions;

public interface IOrderRepository
{
    Task<Domain.Entities.Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Domain.Entities.Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task AddAsync(Domain.Entities.Order order, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
