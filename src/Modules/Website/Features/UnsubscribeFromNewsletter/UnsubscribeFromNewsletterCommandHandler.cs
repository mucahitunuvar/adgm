using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UnsubscribeFromNewsletter;

// ADR-024 §14 (Faz 3 Görev 6): "Abonelikten çıkma, guard gerektirmez (tek tık çalışmalıdır)" - no
// IPublicSubmissionGuard call here, unlike SubscribeToNewsletter. The token is the subscriber's own
// persisted UnsubscribeToken (looked up directly), not a signed/expiring one - see
// NewsletterSubscriber.UnsubscribeToken's remarks for why.
public sealed class UnsubscribeFromNewsletterCommandHandler(
    INewsletterSubscriberRepository newsletterSubscriberRepository,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UnsubscribeFromNewsletterCommand, Result>
{
    private static readonly Error InvalidTokenError = Error.Validation("Newsletter.InvalidUnsubscribeToken", "Invalid unsubscribe token.");

    public async Task<Result> Handle(UnsubscribeFromNewsletterCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return Result.Failure(InvalidTokenError);
        }

        var subscriber = await newsletterSubscriberRepository.GetByUnsubscribeTokenAsync(request.Token, cancellationToken);
        if (subscriber is null)
        {
            return Result.Failure(InvalidTokenError);
        }

        var unsubscribeResult = subscriber.Unsubscribe(timeProvider.GetUtcNow().UtcDateTime);
        if (unsubscribeResult.IsFailure)
        {
            return unsubscribeResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
