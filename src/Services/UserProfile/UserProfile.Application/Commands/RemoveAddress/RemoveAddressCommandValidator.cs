using FluentValidation;

namespace UserProfile.Application.Commands.RemoveAddress;

internal sealed class RemoveAddressCommandValidator : AbstractValidator<RemoveAddressCommand>
{
    public RemoveAddressCommandValidator()
    {
        RuleFor(x => x.KeycloakUserId).NotEmpty();
        RuleFor(x => x.AddressId).NotEmpty();
    }
}
