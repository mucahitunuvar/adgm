using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.SubscribeToNewsletter;

public sealed class SubscribeToNewsletterCommandValidator : AbstractValidator<SubscribeToNewsletterCommand>
{
    public SubscribeToNewsletterCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty();
    }
}
