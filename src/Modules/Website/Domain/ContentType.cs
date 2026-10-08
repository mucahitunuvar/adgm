using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4.1 (Faz 1a Görev 2). RowVersion is the same application-managed optimistic-concurrency
// token as SiteSettings (Faz 0): regenerated on every mutation, compared explicitly by the command
// handler before any change is applied - not a database-generated rowversion column, since Website
// runs on both SqlServer and Sqlite (ADR-012).
//
// Cross-aggregate invariants this type cannot check by itself (Key uniqueness, RoutePrefix
// uniqueness within a language, RoutePrefix vs. SiteLanguage/ReservedRouteSegments collisions, and
// every invariant that depends on whether the type currently has content - ContentItem does not
// exist until Görev 3) are the Application-layer command handlers' responsibility, the same
// separation SiteLanguage's "exactly one default" invariant already uses.
public sealed partial class ContentType : AggregateRoot
{
    public const int MaxTemplateLength = 50;

    private static readonly Regex TemplatePattern = TemplatePatternRegex();

    private readonly List<ContentTypeTranslation> _translations = [];

    public ContentTypeKey Key { get; private set; } = null!;

    public string ListTemplate { get; private set; } = string.Empty;

    public string DetailTemplate { get; private set; } = string.Empty;

    public ContentTypeSortMode SortMode { get; private set; }

    public bool IsActive { get; private set; }

    public int SortOrder { get; private set; }

    // Stored as 14 individual scalar columns, not as one ContentTypeFeatureFlags-typed column: EF
    // Core's migration HasData silently drops a complex-typed property's values regardless of how the
    // seed object shapes them (verified against a real migration - the generated InsertData omitted
    // every Flags column and the seed insert failed NOT NULL constraints twice, both nested and
    // flattened). Flags is a read-only projection of those columns for callers - ApplyFlags/Create's
    // constructor is the only place that writes to them, always together, from a
    // ContentTypeFeatureFlags the caller supplied.
    public bool SupportsHierarchy { get; private set; }

    public bool SupportsCategories { get; private set; }

    public bool SupportsTags { get; private set; }

    public bool SupportsDetailImage { get; private set; }

    public bool SupportsGallery { get; private set; }

    public bool SupportsVideos { get; private set; }

    public bool SupportsAttachments { get; private set; }

    public bool SupportsEvent { get; private set; }

    public bool SupportsBlockLayout { get; private set; }

    public bool SupportsForm { get; private set; }

    public bool SupportsRelatedContent { get; private set; }

    public bool HasDetailPage { get; private set; }

    public bool HasListingPage { get; private set; }

    public bool IsSearchable { get; private set; }

    public bool RequiresReview { get; private set; }

    // ADR-024 §15 (Faz 5 Görev 6): a plain scalar on ContentType itself, deliberately outside
    // ContentTypeFeatureFlags (it is not a boolean capability switch, and unlike every Flags field it
    // carries no cross-field invariant of its own) and outside ContentTypeTranslation (it is not
    // language-dependent). Set independently via SetSchemaKind rather than through Create/Update's
    // flags parameter, so the many existing Create/Update call sites across the test suite are
    // unaffected by this field's addition.
    public ContentSchemaKind SchemaKind { get; private set; } = ContentSchemaKind.None;

    public ContentTypeFeatureFlags Flags => new(
        SupportsHierarchy, SupportsCategories, SupportsTags, SupportsDetailImage, SupportsGallery, SupportsVideos,
        SupportsAttachments, SupportsEvent, SupportsBlockLayout, SupportsForm, SupportsRelatedContent, HasDetailPage,
        HasListingPage, IsSearchable, RequiresReview);

    public IReadOnlyList<ContentTypeTranslation> Translations => _translations.AsReadOnly();

    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    private ContentType(
        Guid id,
        ContentTypeKey key,
        string listTemplate,
        string detailTemplate,
        ContentTypeSortMode sortMode,
        int sortOrder,
        ContentTypeFeatureFlags flags,
        Guid createdByUserId,
        DateTime createdAtUtc)
        : base(id)
    {
        Key = key;
        ListTemplate = listTemplate;
        DetailTemplate = detailTemplate;
        SortMode = sortMode;
        SortOrder = sortOrder;
        ApplyFlags(flags);
        IsActive = true;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    private ContentType()
    {
    }

    public static Result<ContentType> Create(
        ContentTypeKey key,
        string? listTemplate,
        string? detailTemplate,
        ContentTypeSortMode sortMode,
        int sortOrder,
        ContentTypeFeatureFlags flags,
        LanguageCode defaultLanguageCode,
        string? defaultLanguageName,
        string? defaultLanguageRoutePrefix,
        SeoMetadata defaultLanguageSeo,
        Guid createdByUserId,
        DateTime createdAtUtc)
    {
        var flagsResult = ValidateFlags(sortMode, flags);
        if (flagsResult.IsFailure)
        {
            return Result.Failure<ContentType>(flagsResult.Error);
        }

        var listTemplateResult = NormalizeTemplate(listTemplate, "ListTemplate");
        if (listTemplateResult.IsFailure)
        {
            return Result.Failure<ContentType>(listTemplateResult.Error);
        }

        var detailTemplateResult = NormalizeTemplate(detailTemplate, "DetailTemplate");
        if (detailTemplateResult.IsFailure)
        {
            return Result.Failure<ContentType>(detailTemplateResult.Error);
        }

        var translationResult = ContentTypeTranslation.Create(
            defaultLanguageCode, defaultLanguageName, defaultLanguageRoutePrefix, defaultLanguageSeo);
        if (translationResult.IsFailure)
        {
            return Result.Failure<ContentType>(translationResult.Error);
        }

        var listingPageResult = CheckListingPageRequiresRoutePrefix(flags, translationResult.Value.RoutePrefix);
        if (listingPageResult.IsFailure)
        {
            return Result.Failure<ContentType>(listingPageResult.Error);
        }

        var contentType = new ContentType(
            Guid.NewGuid(), key, listTemplateResult.Value, detailTemplateResult.Value, sortMode, sortOrder, flags,
            createdByUserId, createdAtUtc);
        contentType._translations.Add(translationResult.Value);

        return Result.Success(contentType);
    }

    // Templates, sort mode, sort order and feature flags all change together through the same PUT
    // (ADR-024 §4.1 Görev 2's single "structure" endpoint) - RoutePrefix is per-language and stays
    // out of this method (SetTranslation's job).
    public Result Update(
        string? listTemplate,
        string? detailTemplate,
        ContentTypeSortMode sortMode,
        int sortOrder,
        ContentTypeFeatureFlags flags,
        Guid updatedByUserId,
        DateTime updatedAtUtc)
    {
        var flagsResult = ValidateFlags(sortMode, flags);
        if (flagsResult.IsFailure)
        {
            return flagsResult;
        }

        var listTemplateResult = NormalizeTemplate(listTemplate, "ListTemplate");
        if (listTemplateResult.IsFailure)
        {
            return listTemplateResult;
        }

        var detailTemplateResult = NormalizeTemplate(detailTemplate, "DetailTemplate");
        if (detailTemplateResult.IsFailure)
        {
            return detailTemplateResult;
        }

        if (flags.HasListingPage)
        {
            var emptyPrefixTranslation = _translations.FirstOrDefault(t => t.RoutePrefix.Length == 0);
            if (emptyPrefixTranslation is not null)
            {
                return Result.Failure(Error.Conflict(
                    "ContentType.ListingPageRequiresRoutePrefix",
                    $"'{emptyPrefixTranslation.LanguageCode}' has an empty route prefix; a type with an empty prefix in any language cannot have a listing page."));
            }
        }

        ListTemplate = listTemplateResult.Value;
        DetailTemplate = detailTemplateResult.Value;
        SortMode = sortMode;
        SortOrder = sortOrder;
        ApplyFlags(flags);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // Upserts the translation for languageCode - one call per language the caller wants to set.
    public Result SetTranslation(
        LanguageCode languageCode, string? name, string? routePrefix, SeoMetadata seo, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);

        if (existing is not null)
        {
            var previousName = existing.Name;
            var previousRoutePrefix = existing.RoutePrefix;
            var previousSeo = existing.Seo;

            var updateResult = existing.Update(name, routePrefix, seo);
            if (updateResult.IsFailure)
            {
                return updateResult;
            }

            var listingPageResult = CheckListingPageRequiresRoutePrefix(Flags, existing.RoutePrefix);
            if (listingPageResult.IsFailure)
            {
                // Roll back the in-memory mutation, using the values captured before Update ran, so
                // an invalid state never lingers on the aggregate even though nothing has been
                // persisted yet (SaveChanges has not been called). Re-normalizing an
                // already-normalized previous value is a no-op (Slug/SeoMetadata are idempotent).
                existing.Update(previousName, previousRoutePrefix.Length == 0 ? null : previousRoutePrefix, previousSeo);
                return listingPageResult;
            }

            Touch(updatedByUserId, updatedAtUtc);
            return Result.Success();
        }

        var createResult = ContentTypeTranslation.Create(languageCode, name, routePrefix, seo);
        if (createResult.IsFailure)
        {
            return createResult;
        }

        var newListingPageResult = CheckListingPageRequiresRoutePrefix(Flags, createResult.Value.RoutePrefix);
        if (newListingPageResult.IsFailure)
        {
            return newListingPageResult;
        }

        _translations.Add(createResult.Value);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // The default-language translation can never be removed (Application layer resolves which
    // language is default and passes it in - ContentType itself has no SiteLanguage access).
    // Whether the language being removed still has content is an Application-layer check against
    // ContentItem, which does not exist until Görev 3 - see the command handler's remarks.
    public Result RemoveTranslation(LanguageCode languageCode, LanguageCode defaultLanguageCode, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (languageCode == defaultLanguageCode)
        {
            return Result.Failure(Error.Conflict(
                "ContentType.CannotDeleteDefaultTranslation", "The default language's translation cannot be deleted."));
        }

        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is null)
        {
            return Result.Failure(Error.NotFound("ContentType.TranslationNotFound", $"No translation exists for language '{languageCode}'."));
        }

        _translations.Remove(existing);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    public void SetSchemaKind(ContentSchemaKind schemaKind, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        SchemaKind = schemaKind;
        Touch(updatedByUserId, updatedAtUtc);
    }

    public Result Activate(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        IsActive = true;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    public Result Deactivate(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        IsActive = false;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    private static Result CheckListingPageRequiresRoutePrefix(ContentTypeFeatureFlags flags, string routePrefix)
    {
        if (flags.HasListingPage && routePrefix.Length == 0)
        {
            return Result.Failure(Error.Conflict(
                "ContentType.ListingPageRequiresRoutePrefix",
                "A type with HasListingPage cannot have an empty route prefix in any language."));
        }

        return Result.Success();
    }

    private static Result ValidateFlags(ContentTypeSortMode sortMode, ContentTypeFeatureFlags flags)
    {
        if (flags.RequiresReview)
        {
            return Result.Failure(Error.Validation(
                "ContentType.RequiresReviewNotSupported", "RequiresReview is not supported in this version."));
        }

        if (sortMode == ContentTypeSortMode.EventDateAsc && !flags.SupportsEvent)
        {
            return Result.Failure(Error.Validation(
                "ContentType.EventDateAscRequiresSupportsEvent", "SortMode 'EventDateAsc' requires SupportsEvent to be enabled."));
        }

        return Result.Success();
    }

    private static Result<string> NormalizeTemplate(string? value, string fieldName)
    {
        var normalized = (value ?? string.Empty).Trim().ToLowerInvariant();

        if (normalized.Length == 0 || normalized.Length > MaxTemplateLength || !TemplatePattern.IsMatch(normalized))
        {
            return Result.Failure<string>(Error.Validation(
                $"ContentType.{fieldName}Invalid",
                $"{fieldName} is required and must be lowercase letters, digits and single hyphens, at most {MaxTemplateLength} characters."));
        }

        return Result.Success(normalized);
    }

    private void ApplyFlags(ContentTypeFeatureFlags flags)
    {
        SupportsHierarchy = flags.SupportsHierarchy;
        SupportsCategories = flags.SupportsCategories;
        SupportsTags = flags.SupportsTags;
        SupportsDetailImage = flags.SupportsDetailImage;
        SupportsGallery = flags.SupportsGallery;
        SupportsVideos = flags.SupportsVideos;
        SupportsAttachments = flags.SupportsAttachments;
        SupportsEvent = flags.SupportsEvent;
        SupportsBlockLayout = flags.SupportsBlockLayout;
        SupportsForm = flags.SupportsForm;
        SupportsRelatedContent = flags.SupportsRelatedContent;
        HasDetailPage = flags.HasDetailPage;
        HasListingPage = flags.HasListingPage;
        IsSearchable = flags.IsSearchable;
        RequiresReview = flags.RequiresReview;
    }

    private void Touch(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;
        RowVersion = Guid.NewGuid().ToByteArray();
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex TemplatePatternRegex();
}
