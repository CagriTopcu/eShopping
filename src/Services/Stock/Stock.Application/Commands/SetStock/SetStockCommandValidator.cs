using FluentValidation;

namespace Stock.Application.Commands.SetStock;

internal sealed class SetStockCommandValidator : AbstractValidator<SetStockCommand>
{
    public SetStockCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();

        RuleFor(x => x.AvailableQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Available quantity cannot be negative.");
    }
}
