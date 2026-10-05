using FluentValidation;

namespace GenclikMerkezi.Modules.Website.Features.DeleteNewsletterSubscriber;

public sealed class DeleteNewsletterSubscriberCommandValidator : AbstractValidator<DeleteNewsletterSubscriberCommand>
{
    public DeleteNewsletterSubscriberCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}
