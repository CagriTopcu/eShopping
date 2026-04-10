using FluentValidation;

namespace Shipping.Application.Commands.UpdateShipmentStatus;

internal sealed class UpdateShipmentStatusCommandValidator : AbstractValidator<UpdateShipmentStatusCommand>
{
    public UpdateShipmentStatusCommandValidator()
    {
        RuleFor(x => x.ShipmentId).NotEmpty();

        RuleFor(x => x.Action).IsInEnum();

        RuleFor(x => x.TrackingNumber)
            .NotEmpty()
            .MaximumLength(100)
            .When(x => x.Action == ShipmentAction.Ship)
            .WithMessage("Tracking number is required when shipping.");
    }
}
