namespace Basket.Infrastructure.Persistence.Documents;

internal sealed class BasketDocument
{
    public string Username { get; set; } = null!;
    public List<BasketItemDocument> Items { get; set; } = [];
}
