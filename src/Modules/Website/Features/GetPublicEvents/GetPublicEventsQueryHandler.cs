using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.Media;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicEvents;

// ADR-024 §17/§11 (Faz 4 Görev 2): the dedicated public events feed - unlike GetPublicContents (any
// SortMode), this always joins through EventSchedule (an item with no calendar never appears here).
// Cache: the candidate page (Step A, IEventScheduleRepository.SearchPublicAsync) is cached under the
// same TTL-shortening pattern GetPublicContentsQueryHandler uses; registrationState/remainingSpots
// (Step B, GetRegistrationStateInputsByContentItemIdsAsync) are fetched fresh every request, cache hit
// or miss (§1 "kontenjan/durum alanları cache'lenmez").
public sealed class GetPublicEventsQueryHandler(
    IContentTypeRepository contentTypeRepository,
    ISiteLanguageRepository siteLanguageRepository,
    IEventScheduleRepository eventScheduleRepository,
    IContentItemRepository contentItemRepository,
    IMediaAssetRepository mediaAssetRepository,
    IFileStorageService fileStorageService,
    ICacheService cacheService,
    TimeProvider timeProvider)
    : IRequestHandler<GetPublicEventsQuery, Result<PagedResult<PublicEventListItemResponse>>>
{
    public async Task<Result<PagedResult<PublicEventListItemResponse>>> Handle(
        GetPublicEventsQuery request, CancellationToken cancellationToken)
    {
        Guid? contentTypeId = null;
        if (!string.IsNullOrWhiteSpace(request.TypeKey))
        {
            var typeKeyResult = ContentTypeKey.Create(request.TypeKey);
            if (typeKeyResult.IsFailure)
            {
                return Result.Failure<PagedResult<PublicEventListItemResponse>>(
                    Error.NotFound("ContentType.NotFound", $"Content type '{request.TypeKey}' could not be found."));
            }

            var contentType = await contentTypeRepository.GetByKeyAsync(typeKeyResult.Value, cancellationToken);
            if (contentType is null || !contentType.IsActive)
            {
                return Result.Failure<PagedResult<PublicEventListItemResponse>>(
                    Error.NotFound("ContentType.NotFound", $"Content type '{request.TypeKey}' could not be found."));
            }

            if (!contentType.SupportsEvent)
            {
                return Result.Failure<PagedResult<PublicEventListItemResponse>>(
                    Error.Validation("ContentType.EventsNotSupported", $"Content type '{request.TypeKey}' does not support events."));
            }

            contentTypeId = contentType.Id;
        }

        if (!TryParseWindow(request.When, out var window))
        {
            return Result.Failure<PagedResult<PublicEventListItemResponse>>(
                Error.Validation("Event.InvalidTimeWindow", "When must be 'upcoming', 'past' or 'all'."));
        }

        EventFormat? format = null;
        if (!string.IsNullOrWhiteSpace(request.Format))
        {
            if (!Enum.TryParse(request.Format, ignoreCase: true, out EventFormat parsedFormat))
            {
                return Result.Failure<PagedResult<PublicEventListItemResponse>>(
                    Error.Validation("Event.InvalidFormat", "Format must be 'InPerson', 'Online' or 'Hybrid'."));
            }

            format = parsedFormat;
        }

        var activeLanguages = await siteLanguageRepository.GetActiveAsync(cancellationToken);
        var resolvedLanguage = (!string.IsNullOrWhiteSpace(request.Lang)
            ? activeLanguages.FirstOrDefault(l => string.Equals(l.Code.Value, request.Lang, StringComparison.OrdinalIgnoreCase))
            : null) ?? activeLanguages.First(l => l.IsDefault);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var pagedRequest = new PagedRequest { Page = request.Page, PageSize = Math.Min(request.PageSize, 50) };

        var earliestUpcomingTransition = contentTypeId is not null
            ? await contentItemRepository.GetEarliestUpcomingTransitionAsync(contentTypeId.Value, now, cancellationToken)
            : await contentItemRepository.GetEarliestUpcomingTransitionAsync(now, cancellationToken);
        var ttl = ContentCacheTtlCalculator.Calculate(now, [earliestUpcomingTransition]);

        var cacheKey = WebsiteCacheKeys.PublicEventList(
            request.TypeKey ?? "*", resolvedLanguage.Code.Value, format?.ToString(), window.ToString(), request.From?.ToString("O"),
            request.To?.ToString("O"), pagedRequest.Page, pagedRequest.PageSize);

        var paged = await cacheService.GetOrCreateAsync(
            cacheKey,
            ct => eventScheduleRepository.SearchPublicAsync(
                contentTypeId, format, request.From, request.To, window, now, resolvedLanguage.Code, pagedRequest, ct),
            ttl,
            cancellationToken);

        var contentItemIds = paged.Items.Select(i => i.ContentItemId).ToList();
        var stateInputs = await eventScheduleRepository.GetRegistrationStateInputsByContentItemIdsAsync(contentItemIds, cancellationToken);

        var items = new List<PublicEventListItemResponse>();
        foreach (var candidate in paged.Items)
        {
            var coverImage = await BuildImageAsync(candidate.CoverImageMediaId, cancellationToken);
            var (registrationState, remainingSpots) = ResolveState(candidate, stateInputs, now);

            items.Add(new PublicEventListItemResponse(
                candidate.ContentItemId, candidate.Title, candidate.FullPath, coverImage,
                new PublicEventSummaryResponse(
                    candidate.StartsAtUtc, candidate.EndsAtUtc, candidate.Format.ToString(), candidate.VenueName, candidate.IsCancelled,
                    registrationState.ToString(), remainingSpots)));
        }

        return Result.Success(new PagedResult<PublicEventListItemResponse>(items, paged.TotalCount, paged.Page, paged.PageSize));
    }

    private static (EventRegistrationState State, int? RemainingSpots) ResolveState(
        PublicEventListItemCandidate candidate, IReadOnlyDictionary<Guid, EventRegistrationStateInputs> stateInputs, DateTime now)
    {
        if (!stateInputs.TryGetValue(candidate.ContentItemId, out var inputs))
        {
            return (EventRegistrationState.Closed, null);
        }

        var state = EventRegistrationStateResolver.Resolve(
            inputs.IsCancelled, inputs.RegistrationEnabled, inputs.RegistrationOpensAtUtc, inputs.RegistrationClosesAtUtc, inputs.StartsAtUtc,
            inputs.Capacity, inputs.ConfirmedCount, inputs.WaitlistEnabled, now);
        var remainingSpots = inputs.Capacity is { } capacity ? Math.Max(0, capacity - inputs.ConfirmedCount) : (int?)null;

        return (state, remainingSpots);
    }

    private static bool TryParseWindow(string? when, out EventTimeWindow window)
    {
        if (string.IsNullOrWhiteSpace(when))
        {
            window = EventTimeWindow.Upcoming;
            return true;
        }

        switch (when.ToLowerInvariant())
        {
            case "upcoming":
                window = EventTimeWindow.Upcoming;
                return true;
            case "past":
                window = EventTimeWindow.Past;
                return true;
            case "all":
                window = EventTimeWindow.All;
                return true;
            default:
                window = default;
                return false;
        }
    }

    private async Task<PublicEventImageResponse?> BuildImageAsync(Guid? mediaAssetId, CancellationToken cancellationToken)
    {
        if (mediaAssetId is null)
        {
            return null;
        }

        var mediaAsset = await mediaAssetRepository.GetByIdAsync(mediaAssetId.Value, cancellationToken);
        if (mediaAsset is null)
        {
            return null;
        }

        var originalUrl = await fileStorageService.GetUrlAsync(mediaAsset.Original.FileKey, cancellationToken);
        string? small = null;
        string? medium = null;
        string? large = null;

        foreach (var variant in mediaAsset.Variants)
        {
            var url = await fileStorageService.GetUrlAsync(variant.File.FileKey, cancellationToken);
            if (variant.VariantName == MediaAssetVariantNames.Small)
            {
                small = url;
            }
            else if (variant.VariantName == MediaAssetVariantNames.Medium)
            {
                medium = url;
            }
            else if (variant.VariantName == MediaAssetVariantNames.Large)
            {
                large = url;
            }
        }

        return new PublicEventImageResponse(small, medium, large, originalUrl);
    }
}
