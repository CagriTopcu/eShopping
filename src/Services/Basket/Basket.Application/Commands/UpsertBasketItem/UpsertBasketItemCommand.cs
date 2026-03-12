using Basket.Application.DTOs;
using Shared.BuildingBlocks.CQRS;

namespace Basket.Application.Commands.UpsertBasketItem;

public sealed record UpsertBasketItemCommand(
    string Username,
    Guid ProductId,
    int Quantity) : ICommand<BasketResponse>;
