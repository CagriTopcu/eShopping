namespace Basket.Infrastructure.Persistence;

public sealed class BasketOptions
{
    public const string SectionName = "Basket";

    public int RedisTtlDays { get; set; } = 30;
}
