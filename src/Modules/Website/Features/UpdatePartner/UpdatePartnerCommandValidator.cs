using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdatePartner;

public sealed class UpdatePartnerCommandValidator : AbstractValidator<UpdatePartnerCommand>
{
    public UpdatePartnerCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.LogoMediaId).NotEmpty();
        RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);
    }
}
