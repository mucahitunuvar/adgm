using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.ConfirmNewsletterSubscription;

public sealed class ConfirmNewsletterSubscriptionCommandValidator : AbstractValidator<ConfirmNewsletterSubscriptionCommand>
{
    public ConfirmNewsletterSubscriptionCommandValidator()
    {
        RuleFor(c => c.Token).NotEmpty();
    }
}
