using GenclikMerkezi.Contracts.Website;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.ContentPaths;
using GenclikMerkezi.Modules.Website.Application.Events;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.CreateEventRegistration;

// ADR-024 §11.2 (Faz 4 Görev 3). Mirrors SubscribeToNewsletterCommandHandler's overall shape (guard,
// legal-version check, "kayıt sızdırmaz" duplicate handling) but adds the authenticated/anonymous
// fork §1 describes: a logged-in, email-confirmed registrant skips the verification email entirely
// and the capacity decision (EventSchedule.ReserveCapacity, via EventCapacityConcurrencyRetryExecutor)
// runs in this same request; an anonymous one is persisted as PendingVerification, holding no
// capacity, and the decision only happens when VerifyEventRegistrationCommandHandler runs later.
public sealed class CreateEventRegistrationCommandHandler(
    IPublicSubmissionGuard publicSubmissionGuard,
    IContentItemRepository contentItemRepository,
    IContentTypeRepository contentTypeRepository,
    IEventScheduleRepository eventScheduleRepository,
    IEventRegistrationRepository eventRegistrationRepository,
    ISiteSettingsRepository siteSettingsRepository,
    ILegalDocumentRepository legalDocumentRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ContentPathCascadeService contentPathCascadeService,
    ICurrentUserContext currentUserContext,
    IPlatformUserEmailConfirmationLookup emailConfirmationLookup,
    EventCapacityConcurrencyRetryExecutor capacityRetryExecutor,
    EventRegistrationNotifier notifier,
    TimeProvider timeProvider,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateEventRegistrationCommand, Result>
{
    private static readonly Error NotFoundError = Error.NotFound("ContentItem.NotFound", "This content item could not be found.");

    private static readonly Error NotAvailableError = Error.NotFound(
        "Event.NotAvailable", "Event registration is not available for this event.");

    public async Task<Result> Handle(CreateEventRegistrationCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserContext.UserId;
        var isAuthenticated = userId is not null;

        if (!isAuthenticated)
        {
            var guardResult = await publicSubmissionGuard.VerifyAsync(
                new PublicSubmissionGuardRequest(request.SubmissionToken, request.TurnstileToken, request.Website),
                request.RemoteIpAddress, cancellationToken);
            if (guardResult.IsFailure)
            {
                return guardResult;
            }
        }

        var contentItem = await contentItemRepository.GetByIdAsync(request.ContentItemId, cancellationToken);
        if (contentItem is null)
        {
            return Result.Failure(NotFoundError);
        }

        var contentType = await contentTypeRepository.GetByIdAsync(contentItem.ContentTypeId, cancellationToken);
        if (contentType is null || !contentType.IsActive || !contentType.SupportsEvent)
        {
            return Result.Failure(NotFoundError);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (!await contentPathCascadeService.IsVisibleWithAncestorsAsync(contentItem, now, cancellationToken))
        {
            return Result.Failure(NotFoundError);
        }

        var eventSchedule = await eventScheduleRepository.GetByContentItemIdAsync(contentItem.Id, cancellationToken);
        if (eventSchedule is null)
        {
            return Result.Failure(NotFoundError);
        }

        var registrationState = EventRegistrationStateResolver.Resolve(
            eventSchedule.IsCancelled, eventSchedule.RegistrationEnabled, eventSchedule.RegistrationOpensAtUtc,
            eventSchedule.RegistrationClosesAtUtc, eventSchedule.StartsAtUtc, eventSchedule.Capacity, eventSchedule.ConfirmedCount,
            eventSchedule.WaitlistEnabled, now);

        var stateError = registrationState switch
        {
            EventRegistrationState.Cancelled => Error.Conflict("Event.Cancelled", "This event has been cancelled."),
            EventRegistrationState.Full => Error.Conflict("Event.CapacityFull", "This event has reached its capacity."),
            EventRegistrationState.NotOpen or EventRegistrationState.Closed => Error.Conflict(
                "Event.RegistrationClosed", "Registration for this event is not open."),
            _ => (Error?)null,
        };
        if (stateError is not null)
        {
            return Result.Failure(stateError);
        }

        var settings = await siteSettingsRepository.GetAsync(cancellationToken) ?? SiteSettings.CreateDefault();
        var eventPrivacyNoticeKey = settings.EventPrivacyNoticeKey;
        if (eventPrivacyNoticeKey is null)
        {
            return Result.Failure(NotAvailableError);
        }

        var document = await legalDocumentRepository.GetByKeyAsync(eventPrivacyNoticeKey, cancellationToken);
        var effective = document is null ? null : LegalDocumentEffectiveVersionResolver.Resolve(document.Versions, now);
        if (effective is null)
        {
            return Result.Failure(NotAvailableError);
        }

        if (request.AcceptedPrivacyNoticeVersion != effective.VersionNumber)
        {
            return Result.Failure(Error.Conflict(
                "PublicSubmission.LegalVersionChanged",
                "The legal document you accepted has since changed. Please review and accept the current version."));
        }

        var emailResult = EventRegistration.NormalizeEmail(request.Email);
        if (emailResult.IsFailure)
        {
            return Result.Failure(emailResult.Error);
        }

        var activeLanguages = await siteLanguageRepository.GetActiveAsync(cancellationToken);
        var resolvedLanguage = (!string.IsNullOrWhiteSpace(request.Lang)
            ? activeLanguages.FirstOrDefault(l => string.Equals(l.Code.Value, request.Lang, StringComparison.OrdinalIgnoreCase))
            : null) ?? activeLanguages.First(l => l.IsDefault);

        var eventTitle = contentItem.Translations.FirstOrDefault(t => t.LanguageCode == resolvedLanguage.Code)?.Title
            ?? contentItem.Translations.First().Title;

        var existing = await eventRegistrationRepository.GetActiveByContentItemIdAndEmailAsync(
            contentItem.Id, emailResult.Value, cancellationToken);
        if (existing is not null)
        {
            return await HandleDuplicateAsync(existing, isAuthenticated, eventTitle, now, cancellationToken);
        }

        // §1 "UserId yalnızca token'dan alınır" (AGENTS §26) - never from the request body, which has
        // no UserId field at all. A logged-in, email-confirmed registrant skips the verification email.
        var skipVerification = isAuthenticated && await emailConfirmationLookup.IsEmailConfirmedAsync(userId!.Value, cancellationToken);

        string? verificationTokenHash = null;
        DateTime? verificationTokenExpiresAtUtc = null;
        string? rawVerificationToken = null;
        if (!skipVerification)
        {
            rawVerificationToken = EventRegistrationTokens.GenerateRawToken();
            verificationTokenHash = EventRegistrationTokens.Hash(rawVerificationToken);
            verificationTokenExpiresAtUtc = now.Add(EventRegistration.VerificationTokenLifetime);
        }

        var createResult = EventRegistration.Create(
            eventSchedule.Id, contentItem.Id, request.FirstName, request.LastName, emailResult.Value, request.Phone,
            isAuthenticated ? userId : null, resolvedLanguage.Code, eventPrivacyNoticeKey, effective.VersionNumber,
            verificationTokenHash, verificationTokenExpiresAtUtc, EventRegistrationTokens.GenerateRawToken(), now);
        if (createResult.IsFailure)
        {
            return Result.Failure(createResult.Error);
        }

        var registration = createResult.Value;
        eventRegistrationRepository.Add(registration);

        if (skipVerification)
        {
            var reserveOutcome = await capacityRetryExecutor.ExecuteAsync(
                eventSchedule,
                () =>
                {
                    var reserveResult = eventSchedule.ReserveCapacity();
                    return reserveResult.IsFailure
                        ? Result.Failure(reserveResult.Error)
                        : registration.ApplyCapacityDecision(reserveResult.Value, "System", now);
                },
                cancellationToken);

            if (reserveOutcome.IsFailure)
            {
                eventRegistrationRepository.Remove(registration);
                return reserveOutcome;
            }

            registration.ConfirmCapacityDecisionCommitted();
            await notifier.SendCapacityDecisionEmailAsync(registration, eventSchedule, eventTitle, cancellationToken);
            return Result.Success();
        }

        registration.RecordVerificationEmailSent(now);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await notifier.SendVerificationEmailAsync(registration, eventTitle, rawVerificationToken!, cancellationToken);

        return Result.Success();
    }

    // §1 "Yinelenen kayıt" - a uniform response for anonymous callers regardless of which branch
    // actually ran (never reveals whether the email was already registered); a logged-in caller gets
    // an explicit 409 instead, since they are already proven to own that identity.
    private async Task<Result> HandleDuplicateAsync(
        EventRegistration existing, bool isAuthenticated, string eventTitle, DateTime now, CancellationToken cancellationToken)
    {
        if (isAuthenticated)
        {
            return Result.Failure(Error.Conflict("Event.AlreadyRegistered", "You are already registered for this event."));
        }

        if (existing.Status == EventRegistrationStatus.PendingVerification)
        {
            if (existing.CanSendVerificationEmail(now))
            {
                var rawToken = EventRegistrationTokens.GenerateRawToken();
                var reissueResult = existing.ReissueVerificationToken(
                    EventRegistrationTokens.Hash(rawToken), now.Add(EventRegistration.VerificationTokenLifetime), now);
                if (reissueResult.IsSuccess)
                {
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    await notifier.SendVerificationEmailAsync(existing, eventTitle, rawToken, cancellationToken);
                }
            }

            return Result.Success();
        }

        await notifier.SendAlreadyRegisteredNoticeAsync(existing, eventTitle, cancellationToken);
        return Result.Success();
    }
}
