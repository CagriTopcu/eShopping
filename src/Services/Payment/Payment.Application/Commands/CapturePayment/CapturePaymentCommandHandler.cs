using Mapster;
using Microsoft.Extensions.Logging;
using Payment.Application.Abstractions;
using Payment.Application.DTOs;
using Payment.Domain.Errors;
using Shared.BuildingBlocks.CQRS;
using Shared.BuildingBlocks.Results;

namespace Payment.Application.Commands.CapturePayment;

internal sealed class CapturePaymentCommandHandler(
    IPaymentRepository paymentRepository,
    ILogger<CapturePaymentCommandHandler> logger)
    : ICommandHandler<CapturePaymentCommand, PaymentResponse>
{
    public async Task<Result<PaymentResponse>> Handle(CapturePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken);

        if (payment is null)
            return PaymentErrors.NotFound;

        var result = payment.Capture();

        if (result.IsFailure)
            return result.Error;

        await paymentRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Payment captured. PaymentId: {PaymentId}, OrderId: {OrderId}, Status: {Status}",
            payment.Id, payment.OrderId, payment.Status);

        return payment.Adapt<PaymentResponse>();
    }
}
