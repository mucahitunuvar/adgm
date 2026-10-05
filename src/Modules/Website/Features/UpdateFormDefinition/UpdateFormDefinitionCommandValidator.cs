using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UpdateFormDefinition;

public sealed class UpdateFormDefinitionCommandValidator : AbstractValidator<UpdateFormDefinitionCommand>
{
    public UpdateFormDefinitionCommandValidator()
    {
        RuleFor(c => c.RowVersion).NotEmpty();
        RuleFor(c => c.PrivacyNoticeKey).NotEmpty();
        RuleFor(c => c.NotificationEmails).NotNull();
        RuleFor(c => c.ExplicitConsents).NotNull();
        RuleForEach(c => c.ExplicitConsents).ChildRules(consent => consent.RuleFor(c => c.LegalDocumentKey).NotEmpty());
    }
}
