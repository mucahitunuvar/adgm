using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.CreateFormDefinition;

public sealed class CreateFormDefinitionCommandValidator : AbstractValidator<CreateFormDefinitionCommand>
{
    public CreateFormDefinitionCommandValidator()
    {
        RuleFor(c => c.Key).NotEmpty();
        RuleFor(c => c.PrivacyNoticeKey).NotEmpty();
        RuleFor(c => c.NotificationEmails).NotNull();
        RuleFor(c => c.ExplicitConsents).NotNull();
        RuleForEach(c => c.ExplicitConsents).ChildRules(consent => consent.RuleFor(c => c.LegalDocumentKey).NotEmpty());
        RuleFor(c => c.DefaultLanguageTitle).NotEmpty();
        RuleFor(c => c.DefaultLanguageSuccessMessage).NotEmpty();
        RuleFor(c => c.DefaultLanguageSubmitButtonLabel).NotEmpty();
    }
}
