using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.ActivatePartner;

public sealed class ActivatePartnerCommandValidator : AbstractValidator<ActivatePartnerCommand>
{
    public ActivatePartnerCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
