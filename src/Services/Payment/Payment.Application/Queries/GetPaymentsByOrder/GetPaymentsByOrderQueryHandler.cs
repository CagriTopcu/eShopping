using Mapster;
using Microsoft.Extensions.Logging;
using Payment.Application.Abstractions;
using Payment.Application.DTOs;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;

namespace Payment.Application.Queries.GetPaymentsByOrder;

internal sealed class GetPaymentsByOrderQueryHandler(
    IPaymentRepository paymentRepository,
    ILogger<GetPaymentsByOrderQueryHandler> logger)
    : IQueryHandler<GetPaymentsByOrderQuery, IReadOnlyList<PaymentResponse>>
{
    public async Task<Result<IReadOnlyList<PaymentResponse>>> Handle(
        GetPaymentsByOrderQuery request, CancellationToken cancellationToken)
    {
        var payments = await paymentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

        logger.LogDebug(
            "Payments retrieved for order. OrderId: {OrderId}, Count: {Count}",
            request.OrderId, payments.Count);

        return payments.Adapt<List<PaymentResponse>>().AsReadOnly();
    }
}
