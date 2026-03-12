namespace Basket.Infrastructure.Persistence.Documents;

internal sealed class BasketItemDocument
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = null!;
    public int Quantity { get; set; }
}
