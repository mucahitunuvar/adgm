using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.CreatePartner;

public sealed class CreatePartnerCommandValidator : AbstractValidator<CreatePartnerCommand>
{
    public CreatePartnerCommandValidator()
    {
        RuleFor(c => c.LogoMediaId).NotEmpty();
        RuleFor(c => c.SortOrder).GreaterThanOrEqualTo(0);
        RuleFor(c => c.DefaultLanguageName).NotEmpty();
    }
}
