namespace Order.Application.Abstractions;

public interface IPaymentClient
{
    Task<bool> ReserveAsync(Guid orderId, Guid customerId, decimal amount, CancellationToken ct = default);
}
