using Basket.Application.DTOs;
using Shared.BuildingBlocks.CQRS;

namespace Basket.Application.Queries.GetBasket;

public sealed record GetBasketQuery(string Username) : IQuery<BasketResponse>;
