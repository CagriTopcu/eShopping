using Mapster;
using Microsoft.Extensions.Logging;
using Payment.Application.Abstractions;
using Payment.Application.DTOs;
using Payment.Domain.Errors;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;

namespace Payment.Application.Queries.GetPaymentById;

internal sealed class GetPaymentByIdQueryHandler(
    IPaymentRepository paymentRepository,
    ILogger<GetPaymentByIdQueryHandler> logger)
    : IQueryHandler<GetPaymentByIdQuery, PaymentResponse>
{
    public async Task<Result<PaymentResponse>> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken);

        if (payment is null)
            return PaymentErrors.NotFound;

        logger.LogDebug(
            "Payment retrieved. PaymentId: {PaymentId}, Status: {Status}",
            payment.Id, payment.Status);

        return payment.Adapt<PaymentResponse>();
    }
}
