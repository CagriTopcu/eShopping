using Basket.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;

namespace Basket.Application.Commands.DeleteBasket;

internal sealed class DeleteBasketCommandHandler(
    IBasketRepository basketRepository,
    ILogger<DeleteBasketCommandHandler> logger)
    : ICommandHandler<DeleteBasketCommand>
{
    public async Task<Result> Handle(DeleteBasketCommand request, CancellationToken cancellationToken)
    {
        await basketRepository.DeleteAsync(request.Username, cancellationToken);

        logger.LogInformation("Basket deleted for user {Username}", request.Username);

        return Result.Success();
    }
}
