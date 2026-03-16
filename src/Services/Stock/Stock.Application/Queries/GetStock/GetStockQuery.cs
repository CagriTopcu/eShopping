using Shared.BuildingBlocks.CQRS;
using Stock.Application.DTOs;

namespace Stock.Application.Queries.GetStock;

public sealed record GetStockQuery(Guid ProductId) : IQuery<StockResponse>;
