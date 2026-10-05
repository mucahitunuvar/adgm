using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.ConfirmNewsletterSubscription;

// ADR-024 §14 (Faz 3 Görev 6): "Onay e-postasındaki link: imzalı, 7 gün geçerli token. POST
// /api/v1/public/newsletter/confirmations (token) → Active." Idempotent for an already-Active
// subscriber (the link may be opened twice); a stale link pointing at a since-unsubscribed row is
// rejected with the same generic error as a genuinely invalid token, never distinguishing the two.
public sealed class ConfirmNewsletterSubscriptionCommandHandler(
    INewsletterConfirmationLinkGenerator confirmationLinkGenerator,
    INewsletterSubscriberRepository newsletterSubscriberRepository,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ConfirmNewsletterSubscriptionCommand, Result>
{
    private static readonly Error InvalidTokenError = Error.Validation(
        "Newsletter.InvalidConfirmationToken", "Invalid or expired confirmation token.");

    public async Task<Result> Handle(ConfirmNewsletterSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var tokenResult = confirmationLinkGenerator.ValidateToken(request.Token);
        if (tokenResult.IsFailure)
        {
            return Result.Failure(InvalidTokenError);
        }

        var subscriber = await newsletterSubscriberRepository.GetByIdAsync(tokenResult.Value, cancellationToken);
        if (subscriber is null || subscriber.Status == NewsletterSubscriberStatus.Unsubscribed)
        {
            return Result.Failure(InvalidTokenError);
        }

        if (subscriber.Status == NewsletterSubscriberStatus.Active)
        {
            return Result.Success();
        }

        var confirmResult = subscriber.Confirm(timeProvider.GetUtcNow().UtcDateTime);
        if (confirmResult.IsFailure)
        {
            return confirmResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
