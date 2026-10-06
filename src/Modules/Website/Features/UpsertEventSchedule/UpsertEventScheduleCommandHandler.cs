using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.GetEventScheduleByContentItemId;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpsertEventSchedule;

// ADR-024 §11.1 (Faz 4 Görev 1): one PUT creates the EventSchedule if the content item does not have
// one yet, or updates the existing one otherwise - "Dile göre çeviri alanları aynı gövdede" means every
// language's texts travel in the same request rather than a separate per-language endpoint
// (ContentType/ContentItem's own SetTranslation-per-call shape does not apply here). The response
// always carries the fresh RowVersion, since the caller needs it for the next PUT regardless of
// whether this one created or updated.
public sealed class UpsertEventScheduleCommandHandler(
    IContentItemRepository contentItemRepository,
    IContentTypeRepository contentTypeRepository,
    ISiteLanguageRepository siteLanguageRepository,
    IEventScheduleRepository eventScheduleRepository,
    IHtmlContentSanitizer htmlContentSanitizer,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpsertEventScheduleCommand, Result<EventScheduleDetailResponse>>
{
    public async Task<Result<EventScheduleDetailResponse>> Handle(UpsertEventScheduleCommand request, CancellationToken cancellationToken)
    {
        var contentItem = await contentItemRepository.GetByIdAsync(request.ContentItemId, cancellationToken);
        if (contentItem is null)
        {
            return Result.Failure<EventScheduleDetailResponse>(
                Error.NotFound("ContentItem.NotFound", $"Content item '{request.ContentItemId}' could not be found."));
        }

        var contentType = await contentTypeRepository.GetByIdAsync(contentItem.ContentTypeId, cancellationToken);
        if (contentType is null)
        {
            return Result.Failure<EventScheduleDetailResponse>(
                Error.Failure("ContentItem.ContentTypeNotFound", "The content item's content type could not be found."));
        }

        if (!contentType.SupportsEvent)
        {
            return Result.Failure<EventScheduleDetailResponse>(Error.Conflict(
                "Event.ContentTypeDoesNotSupportEvent", "This content item's content type does not support events."));
        }

        if (!Enum.TryParse<EventFormat>(request.Format, ignoreCase: true, out var format))
        {
            return Result.Failure<EventScheduleDetailResponse>(
                Error.Validation("EventSchedule.InvalidFormat", $"'{request.Format}' is not a recognized event format."));
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure<EventScheduleDetailResponse>(
                Error.Failure("EventSchedule.NoDefaultLanguage", "No default site language is configured."));
        }

        var defaultTranslationInput = request.Translations.FirstOrDefault(
            t => string.Equals(t.LanguageCode, defaultLanguage.Code.Value, StringComparison.OrdinalIgnoreCase));
        if (defaultTranslationInput is null)
        {
            return Result.Failure<EventScheduleDetailResponse>(Error.Validation(
                "EventSchedule.DefaultLanguageTranslationRequired", $"A translation for the default language '{defaultLanguage.Code}' is required."));
        }

        var userId = currentUserContext.UserId!.Value;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var existing = await eventScheduleRepository.GetByContentItemIdAsync(request.ContentItemId, cancellationToken);

        EventSchedule schedule;
        if (existing is null)
        {
            var sanitizedDefaultProgramFlow = htmlContentSanitizer.Sanitize(defaultTranslationInput.ProgramFlow ?? string.Empty);

            var createResult = EventSchedule.Create(
                request.ContentItemId, request.StartsAtUtc, request.EndsAtUtc, format, request.OnlineLink, request.Capacity,
                request.RegistrationEnabled, request.RegistrationOpensAtUtc, request.RegistrationClosesAtUtc, request.MinAge,
                request.MaxAge, request.AutoConfirm, request.WaitlistEnabled, defaultLanguage.Code, defaultTranslationInput.VenueName,
                defaultTranslationInput.VenueAddress, defaultTranslationInput.FeeInfo, defaultTranslationInput.Instructors,
                sanitizedDefaultProgramFlow, defaultTranslationInput.AccessibilityNote, userId, now);
            if (createResult.IsFailure)
            {
                return Result.Failure<EventScheduleDetailResponse>(createResult.Error);
            }

            schedule = createResult.Value;
            eventScheduleRepository.Add(schedule);
        }
        else
        {
            if (request.RowVersion is null || !request.RowVersion.SequenceEqual(existing.RowVersion))
            {
                return Result.Failure<EventScheduleDetailResponse>(Error.Conflict(
                    "EventSchedule.ConcurrencyConflict", "The event schedule was changed by someone else. Reload and try again."));
            }

            var updateResult = existing.UpdateSchedule(
                request.StartsAtUtc, request.EndsAtUtc, format, request.OnlineLink, request.Capacity, request.RegistrationEnabled,
                request.RegistrationOpensAtUtc, request.RegistrationClosesAtUtc, request.MinAge, request.MaxAge, request.AutoConfirm,
                request.WaitlistEnabled, now, userId, now);
            if (updateResult.IsFailure)
            {
                return Result.Failure<EventScheduleDetailResponse>(updateResult.Error);
            }

            schedule = existing;
        }

        foreach (var translationInput in request.Translations)
        {
            var languageCodeResult = LanguageCode.Create(translationInput.LanguageCode);
            if (languageCodeResult.IsFailure)
            {
                return Result.Failure<EventScheduleDetailResponse>(languageCodeResult.Error);
            }

            // The default language's translation was already applied by Create above when this is a
            // brand-new schedule - re-applying it here (via SetTranslation) is a harmless no-op since
            // the values are identical, and keeps the loop uniform for both create and update.
            var sanitizedProgramFlow = htmlContentSanitizer.Sanitize(translationInput.ProgramFlow ?? string.Empty);
            var setResult = schedule.SetTranslation(
                languageCodeResult.Value, translationInput.VenueName, translationInput.VenueAddress, translationInput.FeeInfo,
                translationInput.Instructors, sanitizedProgramFlow, translationInput.AccessibilityNote, userId, now);
            if (setResult.IsFailure)
            {
                return Result.Failure<EventScheduleDetailResponse>(setResult.Error);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        var translations = schedule.Translations
            .Select(t => new EventScheduleTranslationResponse(
                t.LanguageCode.Value, t.VenueName, t.VenueAddress, t.FeeInfo, t.Instructors, t.ProgramFlow, t.AccessibilityNote))
            .ToList();

        var response = new EventScheduleDetailResponse(
            schedule.Id, schedule.ContentItemId, schedule.StartsAtUtc, schedule.EndsAtUtc, schedule.Format.ToString(), schedule.OnlineLink,
            schedule.Capacity, schedule.RegistrationEnabled, schedule.RegistrationOpensAtUtc, schedule.RegistrationClosesAtUtc,
            schedule.MinAge, schedule.MaxAge, schedule.AutoConfirm, schedule.WaitlistEnabled, schedule.IsCancelled, schedule.CancelledAtUtc,
            schedule.CancellationReason, schedule.ConfirmedCount, schedule.WaitlistedCount, schedule.RowVersion, translations);

        return Result.Success(response);
    }
}
