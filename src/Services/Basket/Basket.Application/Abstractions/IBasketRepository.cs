namespace Basket.Application.Abstractions;

public interface IBasketRepository
{
    Task<Basket.Domain.Entities.Basket?> GetAsync(string username, CancellationToken ct = default);
    Task SaveAsync(Basket.Domain.Entities.Basket basket, CancellationToken ct = default);
    Task DeleteAsync(string username, CancellationToken ct = default);
}
