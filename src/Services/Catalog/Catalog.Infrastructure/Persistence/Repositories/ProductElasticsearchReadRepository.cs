using Catalog.Application.Abstractions;
using Catalog.Application.DTOs;
using Catalog.Application.ReadModels;
using Catalog.Infrastructure.Persistence.Elasticsearch;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Shared.BuildingBlocks.Pagination;

namespace Catalog.Infrastructure.Persistence.Repositories;

internal sealed class ProductElasticsearchReadRepository(ElasticsearchClient client)
    : IProductReadRepository
{
    public async Task<ProductReadModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var response = await client.GetAsync<ProductReadModel>(
            CatalogIndexMapping.IndexName, id.ToString(), ct);

        if (!response.Found || response.Source is null || response.Source.IsDeleted)
            return null;

        return response.Source;
    }

    public async Task<(IReadOnlyList<ProductReadModel> Items, int TotalCount)> GetPagedAsync(
        string? category,
        string? name,
        decimal? minPrice,
        decimal? maxPrice,
        PaginationParams pagination,
        CancellationToken ct = default)
    {
        var filterActions = new List<Action<QueryDescriptor<ProductReadModel>>>
        {
            f => f.Term(t => t.Field(p => p.IsDeleted).Value(false))
        };

        if (!string.IsNullOrWhiteSpace(category))
        {
            var cat = category;
            filterActions.Add(f => f.Term(t => t.Field(p => p.Category).Value(cat)));
        }

        if (minPrice.HasValue)
        {
            var min = (double)minPrice.Value;
            filterActions.Add(f => f.Range(r => r.NumberRange(
                nr => nr.Field(p => p.Price).Gte(min))));
        }

        if (maxPrice.HasValue)
        {
            var max = (double)maxPrice.Value;
            filterActions.Add(f => f.Range(r => r.NumberRange(
                nr => nr.Field(p => p.Price).Lte(max))));
        }

        var response = await client.SearchAsync<ProductReadModel>(s =>
        {
            s.Index(CatalogIndexMapping.IndexName)
             .From(pagination.Skip)
             .Size(pagination.PageSize)
             .Query(q => q.Bool(b =>
             {
                 b.Filter(filterActions.ToArray());

                 if (!string.IsNullOrWhiteSpace(name))
                 {
                     var n = name;
                     b.Must(m => m.Match(mm => mm
                         .Field(p => p.Name)
                         .Query(n)
                         .Fuzziness(new Fuzziness("AUTO"))));
                 }
             }));
        }, ct);

        if (!response.IsValidResponse)
        {
            return (Array.Empty<ProductReadModel>(), 0);
        }

        var items = response.Documents.ToList();
        var total = (int)response.Total;
        return (items.AsReadOnly(), total);
    }

    public async Task<CatalogStatsResponse> GetStatsAsync(CancellationToken ct = default)
    {
        // Get all active products (for counting and category extraction)
        var allResponse = await client.SearchAsync<ProductReadModel>(s =>
        {
            s.Index(CatalogIndexMapping.IndexName)
             .Size(10000)
             .Query(q => q.Term(t => t.Field(p => p.IsDeleted).Value(false)));
        }, ct);

        if (!allResponse.IsValidResponse)
            return new CatalogStatsResponse(0, 0, []);

        var products = allResponse.Documents.ToList();
        var totalProducts = products.Count;
        var lowStockCount = products.Count(p => p.Stock < 10);
        var categories = products.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();

        return new CatalogStatsResponse(totalProducts, lowStockCount, categories);
    }

    public async Task UpsertAsync(ProductReadModel model, CancellationToken ct = default)
    {
        await client.IndexAsync(
            model,
            r => r.Index(CatalogIndexMapping.IndexName).Id(model.Id.ToString()),
            ct);
    }

    public async Task MarkDeletedAsync(Guid id, CancellationToken ct = default)
    {
        await client.UpdateAsync<ProductReadModel, object>(
            CatalogIndexMapping.IndexName,
            id.ToString(),
            u => u.Doc(new { isDeleted = true, updatedAt = DateTime.UtcNow }),
            ct);
    }
}
