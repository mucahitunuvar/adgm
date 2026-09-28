using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4.2/§4.4 (Faz 1a Görev 3). RowVersion follows the same application-managed optimistic-
// concurrency pattern as SiteSettings/ContentType.
//
// ParentId always stays null in this phase - hierarchy, and the redirect/FullPath-recompute cascade
// that a RoutePrefix or slug change triggers, are Görev 4's scope (ADR-024 §4.3). Everything this type
// cannot check by itself - ContentType existence/flags/RoutePrefix, FullPath uniqueness across the
// whole module, MediaAsset existence/kind, whether the default site language even is what the caller
// claims - is the Application-layer command handler's responsibility, the same separation ContentType
// already uses for its own cross-aggregate invariants.
public sealed class ContentItem : AggregateRoot
{
    private readonly List<ContentItemTranslation> _translations = [];

    public Guid ContentTypeId { get; private set; }

    public Guid? ParentId { get; private set; }

    public ContentItemStatus Status { get; private set; }

    public DateTime? PublishAtUtc { get; private set; }

    public DateTime? UnpublishAtUtc { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsFeatured { get; private set; }

    public Guid? CoverImageMediaId { get; private set; }

    public Guid? DetailImageMediaId { get; private set; }

    public IReadOnlyList<ContentItemTranslation> Translations => _translations.AsReadOnly();

    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    public Guid? PublishedByUserId { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    private ContentItem(
        Guid id, Guid contentTypeId, int sortOrder, bool isFeatured, Guid? coverImageMediaId, Guid? detailImageMediaId,
        Guid createdByUserId, DateTime createdAtUtc)
        : base(id)
    {
        ContentTypeId = contentTypeId;
        Status = ContentItemStatus.Draft;
        SortOrder = sortOrder;
        IsFeatured = isFeatured;
        CoverImageMediaId = coverImageMediaId;
        DetailImageMediaId = detailImageMediaId;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    private ContentItem()
    {
    }

    public static Result<ContentItem> Create(
        Guid contentTypeId,
        bool contentTypeSupportsDetailImage,
        int sortOrder,
        bool isFeatured,
        Guid? coverImageMediaId,
        Guid? detailImageMediaId,
        LanguageCode defaultLanguageCode,
        string? defaultTitle,
        string? defaultSlug,
        string defaultRoutePrefix,
        string? defaultSummary,
        string? defaultBody,
        SeoMetadata defaultSeo,
        Guid createdByUserId,
        DateTime createdAtUtc)
    {
        var detailImageResult = CheckDetailImageAllowed(detailImageMediaId, contentTypeSupportsDetailImage);
        if (detailImageResult.IsFailure)
        {
            return Result.Failure<ContentItem>(detailImageResult.Error);
        }

        var translationResult = ContentItemTranslation.Create(
            defaultLanguageCode, defaultTitle, defaultSlug, defaultRoutePrefix, defaultSummary, defaultBody, defaultSeo);
        if (translationResult.IsFailure)
        {
            return Result.Failure<ContentItem>(translationResult.Error);
        }

        var contentItem = new ContentItem(
            Guid.NewGuid(), contentTypeId, sortOrder, isFeatured, coverImageMediaId, detailImageMediaId,
            createdByUserId, createdAtUtc);
        contentItem._translations.Add(translationResult.Value);

        return Result.Success(contentItem);
    }

    // "sıra, öne çıkan, görseller" (ADR-024 §4.1 Görev 3's single PUT endpoint) - translations are
    // SetTranslation's job, status/scheduling are Publish/Unpublish/Archive/Unarchive/Schedule's.
    public Result UpdateCore(
        int sortOrder, bool isFeatured, Guid? coverImageMediaId, Guid? detailImageMediaId, bool contentTypeSupportsDetailImage,
        Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var detailImageResult = CheckDetailImageAllowed(detailImageMediaId, contentTypeSupportsDetailImage);
        if (detailImageResult.IsFailure)
        {
            return detailImageResult;
        }

        SortOrder = sortOrder;
        IsFeatured = isFeatured;
        CoverImageMediaId = coverImageMediaId;
        DetailImageMediaId = detailImageMediaId;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // Upserts the translation for languageCode - one call per language the caller wants to set.
    // routePrefix is the owning ContentType's current RoutePrefix for this language, resolved by the
    // caller (ContentItem has no ContentType access) and used only to (re)compute FullPath.
    public Result SetTranslation(
        LanguageCode languageCode, string? title, string? slug, string routePrefix, string? summary, string? body,
        SeoMetadata seo, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);

        Result result;
        if (existing is not null)
        {
            result = existing.Update(title, slug, routePrefix, summary, body, seo);
        }
        else
        {
            var createResult = ContentItemTranslation.Create(languageCode, title, slug, routePrefix, summary, body, seo);
            if (createResult.IsSuccess)
            {
                _translations.Add(createResult.Value);
            }

            result = createResult;
        }

        if (result.IsFailure)
        {
            return result;
        }

        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // The default-language translation can never be removed (Application layer resolves which
    // language is default and passes it in - ContentItem itself has no SiteLanguage access).
    public Result RemoveTranslation(LanguageCode languageCode, LanguageCode defaultLanguageCode, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (languageCode == defaultLanguageCode)
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.CannotDeleteDefaultTranslation", "The default language's translation cannot be deleted."));
        }

        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is null)
        {
            return Result.Failure(Error.NotFound("ContentItem.TranslationNotFound", $"No translation exists for language '{languageCode}'."));
        }

        _translations.Remove(existing);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // Covers both the initial publish (Draft -> Published) and a "republish" (Unpublished ->
    // Published) - the master prompt's diagram draws them as two arrows into the same state, but they
    // are the same operation.
    public Result Publish(
        DateTime? publishAtUtc, DateTime? unpublishAtUtc, bool contentTypeIsActive, LanguageCode defaultLanguageCode,
        Guid updatedByUserId, DateTime now)
    {
        if (Status is not (ContentItemStatus.Draft or ContentItemStatus.Unpublished))
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.InvalidStatusTransition", $"Cannot publish content from status '{Status}'."));
        }

        if (!contentTypeIsActive)
        {
            return Result.Failure(Error.Conflict("ContentItem.ContentTypeInactive", "The content type is not active."));
        }

        // Structurally guaranteed by Create/RemoveTranslation (the default-language translation is
        // mandatory and can never be removed) - kept as an explicit, defensive check because it is
        // exactly the rule ADR-024 §4.4 states, not because it can currently fail.
        if (!_translations.Any(t => t.LanguageCode == defaultLanguageCode))
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.MissingDefaultLanguageTranslation", "The default language translation is required to publish."));
        }

        var scheduleResult = ValidateSchedule(publishAtUtc, unpublishAtUtc, now);
        if (scheduleResult.IsFailure)
        {
            return scheduleResult;
        }

        Status = ContentItemStatus.Published;
        PublishAtUtc = publishAtUtc;
        UnpublishAtUtc = unpublishAtUtc;
        PublishedByUserId = updatedByUserId;
        PublishedAtUtc = now;
        Touch(updatedByUserId, now);

        return Result.Success();
    }

    public Result Unpublish(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (Status != ContentItemStatus.Published)
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.InvalidStatusTransition", $"Cannot unpublish content from status '{Status}'."));
        }

        Status = ContentItemStatus.Unpublished;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    public Result Archive(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (Status is not (ContentItemStatus.Published or ContentItemStatus.Unpublished))
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.InvalidStatusTransition", $"Cannot archive content from status '{Status}'."));
        }

        Status = ContentItemStatus.Archived;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // Archived never goes directly back to Published - an editor must review it as Unpublished first
    // (ADR-024 §4.4 as clarified by this master prompt).
    public Result Unarchive(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (Status != ContentItemStatus.Archived)
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.InvalidStatusTransition", $"Cannot unarchive content from status '{Status}'."));
        }

        Status = ContentItemStatus.Unpublished;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // Rescheduling only makes sense for already-published content - Draft/Unpublished/Archived have
    // no active publish window to move.
    public Result Schedule(DateTime? publishAtUtc, DateTime? unpublishAtUtc, Guid updatedByUserId, DateTime now)
    {
        if (Status != ContentItemStatus.Published)
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.InvalidStatusTransition", $"Cannot reschedule content from status '{Status}'."));
        }

        var scheduleResult = ValidateSchedule(publishAtUtc, unpublishAtUtc, now);
        if (scheduleResult.IsFailure)
        {
            return scheduleResult;
        }

        PublishAtUtc = publishAtUtc;
        UnpublishAtUtc = unpublishAtUtc;
        Touch(updatedByUserId, now);

        return Result.Success();
    }

    // ADR-024 §4.4's single definition of "visible on the public site" - every future public query
    // (Görev 6 onward) is meant to build on this exact condition, not re-derive it. now is a parameter,
    // not DateTime.UtcNow read internally, so callers (and their tests) can fix it.
    public bool IsVisible(DateTime now) =>
        Status == ContentItemStatus.Published
        && (PublishAtUtc is null || PublishAtUtc <= now)
        && (UnpublishAtUtc is null || UnpublishAtUtc > now);

    private static Result ValidateSchedule(DateTime? publishAtUtc, DateTime? unpublishAtUtc, DateTime now)
    {
        if (unpublishAtUtc is null)
        {
            return Result.Success();
        }

        var effectivePublishStart = publishAtUtc ?? now;
        if (unpublishAtUtc <= effectivePublishStart)
        {
            return Result.Failure(Error.Validation(
                "ContentItem.UnpublishBeforePublish", "UnpublishAtUtc must be after PublishAtUtc (or after now, when publishing immediately)."));
        }

        return Result.Success();
    }

    private static Result CheckDetailImageAllowed(Guid? detailImageMediaId, bool contentTypeSupportsDetailImage)
    {
        if (detailImageMediaId is not null && !contentTypeSupportsDetailImage)
        {
            return Result.Failure(Error.Validation(
                "ContentItem.DetailImageNotSupported", "The content type does not support a detail image."));
        }

        return Result.Success();
    }

    private void Touch(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;
        RowVersion = Guid.NewGuid().ToByteArray();
    }
}
