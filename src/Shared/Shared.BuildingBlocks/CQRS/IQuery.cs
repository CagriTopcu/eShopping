using MediatR;
using Shared.BuildingBlocks.Results;

namespace Shared.BuildingBlocks.CQRS;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
