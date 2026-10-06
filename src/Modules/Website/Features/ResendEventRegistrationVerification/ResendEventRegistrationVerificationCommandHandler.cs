using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.Events;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.ResendEventRegistrationVerification;

// ADR-024 §11.2 (Faz 4 Görev 3): "tekdüze 202; kayıt başına 10 dakikada en fazla 1 gönderim" - every
// branch (no such registration, not PendingVerification, within the resend cooldown, or an actual
// resend) returns the same Result.Success(), the same "kayıt sızdırmaz" shape
// SubscribeToNewsletterCommandHandler's own resend branch uses.
public sealed class ResendEventRegistrationVerificationCommandHandler(
    IPublicSubmissionGuard publicSubmissionGuard,
    IEventRegistrationRepository eventRegistrationRepository,
    IContentItemRepository contentItemRepository,
    EventRegistrationNotifier notifier,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ResendEventRegistrationVerificationCommand, Result>
{
    public async Task<Result> Handle(ResendEventRegistrationVerificationCommand request, CancellationToken cancellationToken)
    {
        var guardResult = await publicSubmissionGuard.VerifyAsync(
            new PublicSubmissionGuardRequest(request.SubmissionToken, request.TurnstileToken, request.Website),
            request.RemoteIpAddress, cancellationToken);
        if (guardResult.IsFailure)
        {
            return guardResult;
        }

        var emailResult = EventRegistration.NormalizeEmail(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure(emailResult.Error);
        }

        var registration = await eventRegistrationRepository.GetActiveByContentItemIdAndEmailAsync(
            request.ContentItemId, emailResult.Value, cancellationToken);
        if (registration is null || registration.Status != EventRegistrationStatus.PendingVerification)
        {
            return Result.Success();
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (!registration.CanSendVerificationEmail(now))
        {
            return Result.Success();
        }

        var rawToken = EventRegistrationTokens.GenerateRawToken();
        var reissueResult = registration.ReissueVerificationToken(
            EventRegistrationTokens.Hash(rawToken), now.Add(EventRegistration.VerificationTokenLifetime), now);
        if (reissueResult.IsFailure)
        {
            return Result.Success();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var contentItem = await contentItemRepository.GetByIdAsync(request.ContentItemId, cancellationToken);
        var eventTitle = contentItem?.Translations.FirstOrDefault(t => t.LanguageCode == registration.LanguageCode)?.Title
            ?? contentItem?.Translations.FirstOrDefault()?.Title ?? string.Empty;

        await notifier.SendVerificationEmailAsync(registration, eventTitle, rawToken, cancellationToken);

        return Result.Success();
    }
}
