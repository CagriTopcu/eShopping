using Shared.BuildingBlocks.CQRS;
using Stock.Application.DTOs;

namespace Stock.Application.Commands.SetStock;

public sealed record SetStockCommand(Guid ProductId, int AvailableQuantity) : ICommand<StockResponse>;
