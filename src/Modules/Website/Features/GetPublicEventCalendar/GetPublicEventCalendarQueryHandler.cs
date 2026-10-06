using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.ContentPaths;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicEventCalendar;

// ADR-024 §11/§17 (Faz 4 Görev 2): the public .ics download - same visibility gate as
// GetPublicContentByIdQueryHandler (self + every ancestor visible), plus SupportsEvent and an actual
// EventSchedule. No caching (§33: no demonstrated need for a single-row, rarely-hit lookup) and no
// registrationState/capacity data at all - the ICS file carries only what ADR-024 §11 lists, which
// excludes OnlineLink and every bit of personal data by design.
public sealed class GetPublicEventCalendarQueryHandler(
    IContentItemRepository contentItemRepository,
    IContentTypeRepository contentTypeRepository,
    IEventScheduleRepository eventScheduleRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ContentPathCascadeService contentPathCascadeService,
    IConfiguration configuration,
    TimeProvider timeProvider)
    : IRequestHandler<GetPublicEventCalendarQuery, Result<string>>
{
    private const string FallbackUidDomain = "genclikmerkezi.invalid";

    private static readonly Error NotFoundError = Error.NotFound("ContentItem.NotFound", "This content item could not be found.");

    public async Task<Result<string>> Handle(GetPublicEventCalendarQuery request, CancellationToken cancellationToken)
    {
        var contentItem = await contentItemRepository.GetByIdAsync(request.ContentItemId, cancellationToken);
        if (contentItem is null)
        {
            return Result.Failure<string>(NotFoundError);
        }

        var contentType = await contentTypeRepository.GetByIdAsync(contentItem.ContentTypeId, cancellationToken);
        if (contentType is null || !contentType.IsActive || !contentType.SupportsEvent || !contentType.HasDetailPage)
        {
            return Result.Failure<string>(NotFoundError);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (!await contentPathCascadeService.IsVisibleWithAncestorsAsync(contentItem, now, cancellationToken))
        {
            return Result.Failure<string>(NotFoundError);
        }

        var eventSchedule = await eventScheduleRepository.GetByContentItemIdAsync(contentItem.Id, cancellationToken);
        if (eventSchedule is null)
        {
            return Result.Failure<string>(NotFoundError);
        }

        var activeLanguages = await siteLanguageRepository.GetActiveAsync(cancellationToken);
        var resolvedLanguage = (!string.IsNullOrWhiteSpace(request.Lang)
            ? activeLanguages.FirstOrDefault(l => string.Equals(l.Code.Value, request.Lang, StringComparison.OrdinalIgnoreCase))
            : null) ?? activeLanguages.First(l => l.IsDefault);
        var defaultLanguage = activeLanguages.First(l => l.IsDefault);

        var translation = contentItem.Translations.FirstOrDefault(t => t.LanguageCode == resolvedLanguage.Code);
        if (translation is null)
        {
            return Result.Failure<string>(NotFoundError);
        }

        var publicSiteBaseUrl = (configuration["Website:PublicSiteBaseUrl"] ?? string.Empty).TrimEnd('/');
        var path = RoutePathFormat.BuildPublicPath(resolvedLanguage.Code.Value, defaultLanguage.Code.Value, translation.FullPath);
        var eventPageUrl = $"{publicSiteBaseUrl}{path}";
        var uidDomain = TryGetHost(publicSiteBaseUrl) ?? FallbackUidDomain;

        var scheduleTranslation = eventSchedule.Translations.FirstOrDefault(t => t.LanguageCode == resolvedLanguage.Code);
        var location = eventSchedule.Format == EventFormat.Online ? "Online" : scheduleTranslation?.VenueName ?? string.Empty;

        var ics = IcsEventCalendarBuilder.Build(
            contentItem.Id, uidDomain, translation.Title, translation.Summary, eventPageUrl, eventSchedule.StartsAtUtc,
            eventSchedule.EndsAtUtc, location, eventSchedule.IsCancelled, now);

        return Result.Success(ics);
    }

    private static string? TryGetHost(string baseUrl) => Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) ? uri.Host : null;
}
