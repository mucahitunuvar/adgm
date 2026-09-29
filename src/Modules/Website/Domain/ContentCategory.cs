using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4.1 (Faz 1b Görev 3): per-type, at most 2 levels deep, translatable. This type only
// enforces what it can check alone (a required default-language translation) - whether ContentTypeId
// actually SupportsCategories, whether ParentId belongs to the same type, whether ParentId is itself
// already a child (the "max 2 levels" rule), and slug uniqueness within (type, language) are all
// cross-aggregate checks the Application-layer command handler makes, the same separation ContentType
// and ContentItem already use for their own invariants.
public sealed class ContentCategory : AggregateRoot
{
    private readonly List<ContentCategoryTranslation> _translations = [];

    public Guid ContentTypeId { get; private set; }

    public Guid? ParentId { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyList<ContentCategoryTranslation> Translations => _translations.AsReadOnly();

    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    private ContentCategory(
        Guid id, Guid contentTypeId, Guid? parentId, int sortOrder, Guid createdByUserId, DateTime createdAtUtc)
        : base(id)
    {
        ContentTypeId = contentTypeId;
        ParentId = parentId;
        SortOrder = sortOrder;
        IsActive = true;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    private ContentCategory()
    {
    }

    public static Result<ContentCategory> Create(
        Guid contentTypeId,
        Guid? parentId,
        int sortOrder,
        LanguageCode defaultLanguageCode,
        string? defaultLanguageName,
        string? defaultLanguageSlug,
        SeoMetadata defaultLanguageSeo,
        Guid createdByUserId,
        DateTime createdAtUtc)
    {
        var translationResult = ContentCategoryTranslation.Create(defaultLanguageCode, defaultLanguageName, defaultLanguageSlug, defaultLanguageSeo);
        if (translationResult.IsFailure)
        {
            return Result.Failure<ContentCategory>(translationResult.Error);
        }

        var category = new ContentCategory(Guid.NewGuid(), contentTypeId, parentId, sortOrder, createdByUserId, createdAtUtc);
        category._translations.Add(translationResult.Value);

        return Result.Success(category);
    }

    public Result UpdateCore(int sortOrder, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        SortOrder = sortOrder;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // Upserts the translation for languageCode - one call per language the caller wants to set.
    public Result SetTranslation(LanguageCode languageCode, string? name, string? slug, SeoMetadata seo, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is not null)
        {
            var updateResult = existing.Update(name, slug, seo);
            if (updateResult.IsFailure)
            {
                return updateResult;
            }

            Touch(updatedByUserId, updatedAtUtc);
            return Result.Success();
        }

        var createResult = ContentCategoryTranslation.Create(languageCode, name, slug, seo);
        if (createResult.IsFailure)
        {
            return createResult;
        }

        _translations.Add(createResult.Value);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // The default-language translation can never be removed (Application layer resolves which
    // language is default and passes it in - ContentCategory itself has no SiteLanguage access).
    public Result RemoveTranslation(LanguageCode languageCode, LanguageCode defaultLanguageCode, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (languageCode == defaultLanguageCode)
        {
            return Result.Failure(Error.Conflict(
                "ContentCategory.CannotDeleteDefaultTranslation", "The default language's translation cannot be deleted."));
        }

        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is null)
        {
            return Result.Failure(Error.NotFound("ContentCategory.TranslationNotFound", $"No translation exists for language '{languageCode}'."));
        }

        _translations.Remove(existing);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
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

    private void Touch(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;
        RowVersion = Guid.NewGuid().ToByteArray();
    }
}
