using Shared.BuildingBlocks.CQRS;

namespace Order.Application.Commands.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId, Guid CustomerId, bool IsAdmin = false) : ICommand;
