using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.DeactivatePartner;

public sealed class DeactivatePartnerCommandValidator : AbstractValidator<DeactivatePartnerCommand>
{
    public DeactivatePartnerCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
    }
}
