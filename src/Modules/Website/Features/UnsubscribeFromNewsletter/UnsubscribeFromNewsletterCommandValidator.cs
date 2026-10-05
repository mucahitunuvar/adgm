using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.UnsubscribeFromNewsletter;

public sealed class UnsubscribeFromNewsletterCommandValidator : AbstractValidator<UnsubscribeFromNewsletterCommand>
{
    public UnsubscribeFromNewsletterCommandValidator()
    {
        RuleFor(c => c.Token).NotEmpty();
    }
}
