using Catalog.Application.DTOs;
using Shared.BuildingBlocks.CQRS;

namespace Catalog.Application.Queries.GetCatalogStats;

public sealed record GetCatalogStatsQuery : IQuery<CatalogStatsResponse>;
