using Catalog.Application.ReadModels;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Elasticsearch;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Seeding;

namespace Catalog.Infrastructure.Seeding;

internal sealed class CatalogDataSeeder(
    IServiceScopeFactory scopeFactory,
    ILogger<CatalogDataSeeder> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var esClient = scope.ServiceProvider.GetRequiredService<ElasticsearchClient>();

        var countResponse = await esClient.CountAsync<ProductReadModel>(
            CatalogIndexMapping.IndexName, cancellationToken);

        if (countResponse.IsValidResponse && countResponse.Count > 0)
        {
            logger.LogInformation("Catalog already seeded with {Count} products. Skipping.", countResponse.Count);
            return;
        }

        logger.LogInformation("Seeding catalog with sample data...");

        var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var now = DateTime.UtcNow;

        foreach (var seed in SeedProductCatalog.Products)
        {
            var result = Product.Reconstitute(
                seed.ProductId, seed.Name, seed.Price, seed.Currency,
                seed.Category, seed.Description, seed.ImageUrl,
                now, updatedAt: null, isDeleted: false, deletedAt: null);

            if (result.IsFailure)
            {
                logger.LogWarning("Failed to create seed product {Name}: {Error}", seed.Name, result.Error);
                continue;
            }

            var product = result.Value;

            var doc = new ProductDocument
            {
                Id = product.Id.Value,
                Name = product.Name.Value,
                Price = product.Price.Amount,
                Currency = product.Price.Currency,
                Category = product.Category.Name,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                IsDeleted = product.IsDeleted,
                DeletedAt = product.DeletedAt
            };

            var readModel = new ProductReadModel
            {
                Id = product.Id.Value,
                Name = product.Name.Value,
                Category = product.Category.Name,
                Price = product.Price.Amount,
                Currency = product.Price.Currency,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                IsDeleted = false
            };

            await dbContext.Products.InsertOneAsync(doc, cancellationToken: cancellationToken);
            await esClient.IndexAsync(
                readModel,
                r => r.Index(CatalogIndexMapping.IndexName).Id(readModel.Id.ToString()),
                cancellationToken);
        }

        logger.LogInformation("Catalog seeded with {Count} products.", SeedProductCatalog.Products.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
