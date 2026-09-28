using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4.2/§4.4 (Faz 1a Görev 3, hierarchy/path-cascade added in Görev 4). RowVersion follows the
// same application-managed optimistic-concurrency pattern as SiteSettings/ContentType.
//
// Everything this type cannot check by itself - ContentType existence/flags/RoutePrefix, FullPath
// uniqueness across the whole module, hierarchy depth/cycle checks against sibling aggregates,
// MediaAsset existence/kind, whether the default site language even is what the caller claims - is the
// Application-layer command handler's responsibility (using ContentPathService for the pure hierarchy/
// path math), the same separation ContentType already uses for its own cross-aggregate invariants.
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
        Guid id, Guid contentTypeId, Guid? parentId, int sortOrder, bool isFeatured, Guid? coverImageMediaId,
        Guid? detailImageMediaId, Guid createdByUserId, DateTime createdAtUtc)
        : base(id)
    {
        ContentTypeId = contentTypeId;
        ParentId = parentId;
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

    // ancestorSlugsRootToParent: every ancestor's slug in defaultLanguageCode, root-to-immediate-
    // parent order (empty for a root-level item) - the caller (Application layer) resolves this and
    // validates the hierarchy itself (ContentPathService.ValidateParentAssignment) before calling
    // Create; this method trusts parentId is already valid.
    public static Result<ContentItem> Create(
        Guid contentTypeId,
        Guid? parentId,
        bool contentTypeSupportsDetailImage,
        int sortOrder,
        bool isFeatured,
        Guid? coverImageMediaId,
        Guid? detailImageMediaId,
        LanguageCode defaultLanguageCode,
        string? defaultTitle,
        string? defaultSlug,
        string defaultRoutePrefix,
        IReadOnlyList<string> ancestorSlugsRootToParent,
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
            defaultLanguageCode, defaultTitle, defaultSlug, defaultRoutePrefix, ancestorSlugsRootToParent,
            defaultSummary, defaultBody, defaultSeo);
        if (translationResult.IsFailure)
        {
            return Result.Failure<ContentItem>(translationResult.Error);
        }

        var contentItem = new ContentItem(
            Guid.NewGuid(), contentTypeId, parentId, sortOrder, isFeatured, coverImageMediaId, detailImageMediaId,
            createdByUserId, createdAtUtc);
        contentItem._translations.Add(translationResult.Value);

        return Result.Success(contentItem);
    }

    // "sıra, öne çıkan, görseller" (ADR-024 §4.1 Görev 3's single PUT endpoint) - translations are
    // SetTranslation's job, status/scheduling are Publish/Unpublish/Archive/Unarchive/Schedule's,
    // parent is SetParent's (Görev 4).
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

    // The caller has already run ContentPathService.ValidateParentAssignment and recomputed this
    // item's (and every descendant's) FullPath/Redirects for the new position - this method only
    // records the new parent itself.
    public void SetParent(Guid? parentId, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        ParentId = parentId;
        Touch(updatedByUserId, updatedAtUtc);
    }

    // Upserts the translation for languageCode - one call per language the caller wants to set.
    // routePrefix/ancestorSlugsRootToParent describe the owning ContentType/ancestor chain's current
    // state for this language, resolved by the caller (ContentItem has no ContentType/repository
    // access) and used only to (re)compute FullPath.
    public Result SetTranslation(
        LanguageCode languageCode, string? title, string? slug, string routePrefix,
        IReadOnlyList<string> ancestorSlugsRootToParent, string? summary, string? body, SeoMetadata seo,
        Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);

        Result result;
        if (existing is not null)
        {
            result = existing.Update(title, slug, routePrefix, ancestorSlugsRootToParent, summary, body, seo);
        }
        else
        {
            var createResult = ContentItemTranslation.Create(
                languageCode, title, slug, routePrefix, ancestorSlugsRootToParent, summary, body, seo);
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

    // Called by the Application-layer path cascade (Görev 4) when an ancestor's RoutePrefix or slug
    // changed and this item's own translation text did not - only FullPath moves, nothing else, and
    // only for languages this item actually has (RemoveTranslation/SetTranslation's own-language rule
    // means a language absent here is absent on every descendant too, so callers simply skip this item
    // for that language rather than treating a missing translation as an error).
    internal void RecomputeFullPath(
        LanguageCode languageCode, string routePrefix, IReadOnlyList<string> ancestorSlugsRootToParent,
        Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var translation = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (translation is null)
        {
            return;
        }

        translation.RecomputeFullPath(routePrefix, ancestorSlugsRootToParent);
        Touch(updatedByUserId, updatedAtUtc);
    }

    // Covers both the initial publish (Draft -> Published) and a "republish" (Unpublished ->
    // Published) - the master prompt's diagram draws them as two arrows into the same state, but they
    // are the same operation. parentIsPublished is irrelevant (and ignored) for a root-level item.
    public Result Publish(
        DateTime? publishAtUtc, DateTime? unpublishAtUtc, bool contentTypeIsActive, bool parentIsPublished,
        LanguageCode defaultLanguageCode, Guid updatedByUserId, DateTime now)
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

        if (ParentId is not null && !parentIsPublished)
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.ParentNotPublished", "The parent content item must be published first."));
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

    public Result Unpublish(int publishedChildCount, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (Status != ContentItemStatus.Published)
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.InvalidStatusTransition", $"Cannot unpublish content from status '{Status}'."));
        }

        if (publishedChildCount > 0)
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.HasPublishedChildren",
                $"Cannot unpublish: {publishedChildCount} published child content item(s) depend on this being published."));
        }

        Status = ContentItemStatus.Unpublished;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    public Result Archive(int publishedChildCount, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (Status is not (ContentItemStatus.Published or ContentItemStatus.Unpublished))
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.InvalidStatusTransition", $"Cannot archive content from status '{Status}'."));
        }

        if (publishedChildCount > 0)
        {
            return Result.Failure(Error.Conflict(
                "ContentItem.HasPublishedChildren",
                $"Cannot archive: {publishedChildCount} published child content item(s) depend on this being published."));
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

    // ADR-024 §4.4's single definition of "visible on the public site" for THIS item alone - it does
    // not consider ancestors (Görev 4: "içerik ancak kendisi ve tüm ataları görünürse görünür"), which
    // is a separate, Application-layer check across multiple aggregates (IsVisible has no ancestor
    // access to do it itself). now is a parameter, not DateTime.UtcNow read internally, so callers
    // (and their tests) can fix it.
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
