using Catalog.Domain.Errors;
using Catalog.Domain.Events;
using Catalog.Domain.ValueObjects;
using Shared.BuildingBlocks.Domain.Primitives;
using Shared.BuildingBlocks.Results;

namespace Catalog.Domain.Entities;

public sealed class Product : AggregateRoot<ProductId>, IAuditableEntity, ISoftDeletable
{
    private Product() : base(ProductId.New()) { }

    private Product(
        ProductId id,
        ProductName name,
        Money price,
        Category category,
        string? description,
        string? imageUrl) : base(id)
    {
        Name = name;
        Price = price;
        Category = category;
        Description = description;
        ImageUrl = imageUrl;
        CreatedAt = DateTime.UtcNow;
    }

    private Product(
        ProductId id,
        ProductName name,
        Money price,
        Category category,
        string? description,
        string? imageUrl,
        DateTime createdAt,
        DateTime? updatedAt,
        bool isDeleted,
        DateTime? deletedAt) : base(id)
    {
        Name = name;
        Price = price;
        Category = category;
        Description = description;
        ImageUrl = imageUrl;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        IsDeleted = isDeleted;
        DeletedAt = deletedAt;
    }

    public static Result<Product> Reconstitute(
        Guid id,
        string name,
        decimal price,
        string currency,
        string category,
        string? description,
        string? imageUrl,
        DateTime createdAt,
        DateTime? updatedAt,
        bool isDeleted,
        DateTime? deletedAt)
    {
        var nameResult = ProductName.Create(name);
        if (nameResult.IsFailure) return nameResult.Error;

        var priceResult = Money.Create(price, currency);
        if (priceResult.IsFailure) return priceResult.Error;

        var categoryResult = Category.Create(category);
        if (categoryResult.IsFailure) return categoryResult.Error;

        return new Product(
            ProductId.From(id),
            nameResult.Value,
            priceResult.Value,
            categoryResult.Value,
            description,
            imageUrl,
            createdAt,
            updatedAt,
            isDeleted,
            deletedAt);
    }

    public ProductName Name { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
    public Category Category { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? ImageUrl { get; private set; }
    public DateTime CreatedAt { get; private init; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public static Result<Product> Create(
        string name,
        decimal price,
        string currency,
        string category,
        string? description,
        string? imageUrl)
    {
        var nameResult = ProductName.Create(name);
        if (nameResult.IsFailure) return nameResult.Error;

        var priceResult = Money.Create(price, currency);
        if (priceResult.IsFailure) return priceResult.Error;

        var categoryResult = Category.Create(category);
        if (categoryResult.IsFailure) return categoryResult.Error;

        var id = ProductId.New();
        var product = new Product(
            id,
            nameResult.Value,
            priceResult.Value,
            categoryResult.Value,
            description,
            imageUrl);

        product.RaiseDomainEvent(new ProductCreatedDomainEvent(id));

        return product;
    }

    public Result UpdateDetails(
        string name,
        decimal price,
        string currency,
        string category,
        string? description,
        string? imageUrl)
    {
        var nameResult = ProductName.Create(name);
        if (nameResult.IsFailure) return nameResult.Error;

        var priceResult = Money.Create(price, currency);
        if (priceResult.IsFailure) return priceResult.Error;

        var categoryResult = Category.Create(category);
        if (categoryResult.IsFailure) return categoryResult.Error;

        var oldPrice = Price;

        Name = nameResult.Value;
        Category = categoryResult.Value;
        Description = description;
        ImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ProductUpdatedDomainEvent(Id));

        if (oldPrice != priceResult.Value)
        {
            Price = priceResult.Value;
            RaiseDomainEvent(new ProductPriceChangedDomainEvent(Id, oldPrice, priceResult.Value));
        }

        return Result.Success();
    }

    public Result Delete()
    {
        if (IsDeleted)
            return ProductErrors.AlreadyDeleted;

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new ProductDeletedDomainEvent(Id));

        return Result.Success();
    }
}
