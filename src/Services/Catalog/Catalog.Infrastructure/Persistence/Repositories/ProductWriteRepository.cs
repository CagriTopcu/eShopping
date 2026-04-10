using Catalog.Application.Abstractions;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using MongoDB.Driver;

namespace Catalog.Infrastructure.Persistence.Repositories;

internal sealed class ProductWriteRepository(CatalogDbContext dbContext) : IProductWriteRepository
{
    public async Task<Product?> GetByIdAsync(ProductId id, CancellationToken ct = default)
    {
        var filter = Builders<ProductDocument>.Filter.And(
            Builders<ProductDocument>.Filter.Eq(d => d.Id, id.Value),
            Builders<ProductDocument>.Filter.Eq(d => d.IsDeleted, false));

        var doc = await dbContext.Products.Find(filter).FirstOrDefaultAsync(ct);
        return doc is null ? null : MapToDomain(doc);
    }

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        var doc = MapToDocument(product);
        await dbContext.Products.InsertOneAsync(doc, cancellationToken: ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        var doc = MapToDocument(product);
        var filter = Builders<ProductDocument>.Filter.Eq(d => d.Id, product.Id.Value);
        await dbContext.Products.ReplaceOneAsync(filter, doc, cancellationToken: ct);
    }

    private static ProductDocument MapToDocument(Product p) => new()
    {
        Id = p.Id.Value,
        Name = p.Name.Value,
        Price = p.Price.Amount,
        Currency = p.Price.Currency,
        Category = p.Category.Name,
        Description = p.Description,
        ImageUrl = p.ImageUrl,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        IsDeleted = p.IsDeleted,
        DeletedAt = p.DeletedAt
    };

    private static Product MapToDomain(ProductDocument doc) =>
        Product.Reconstitute(
            doc.Id,
            doc.Name,
            doc.Price,
            doc.Currency,
            doc.Category,
            doc.Description,
            doc.ImageUrl,
            doc.CreatedAt,
            doc.UpdatedAt,
            doc.IsDeleted,
            doc.DeletedAt).Value;
}
