using Basket.Application.Abstractions;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;

namespace Basket.Application.Commands.DeleteBasket;

internal sealed class DeleteBasketCommandHandler(IBasketRepository basketRepository)
    : ICommandHandler<DeleteBasketCommand>
{
    public async Task<Result> Handle(DeleteBasketCommand request, CancellationToken cancellationToken)
    {
        await basketRepository.DeleteAsync(request.Username, cancellationToken);
        return Result.Success();
    }
}
