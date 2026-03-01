using MediatR;
using Shared.BuildingBlocks.Results;

namespace Shared.BuildingBlocks.CQRS;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
