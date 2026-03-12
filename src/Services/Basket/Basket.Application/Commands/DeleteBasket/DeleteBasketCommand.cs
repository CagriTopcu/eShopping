using Shared.BuildingBlocks.CQRS;

namespace Basket.Application.Commands.DeleteBasket;

public sealed record DeleteBasketCommand(string Username) : ICommand;
