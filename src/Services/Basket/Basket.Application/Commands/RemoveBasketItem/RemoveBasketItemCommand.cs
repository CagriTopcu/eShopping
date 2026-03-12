using Shared.BuildingBlocks.CQRS;

namespace Basket.Application.Commands.RemoveBasketItem;

public sealed record RemoveBasketItemCommand(string Username, Guid ProductId) : ICommand;
