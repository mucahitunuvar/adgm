using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §13 (Faz 3 Görev 7). Mirrors Partner's shape (AGENTS.md §10: RowVersion is the same
// application-managed optimistic-concurrency token every other aggregate uses).
public sealed class ThirdPartyScript : AggregateRoot
{
    private readonly List<ThirdPartyScriptTranslation> _translations = [];

    public ThirdPartyScriptProvider Provider { get; private set; } = null!;

    public ThirdPartyScriptCategory Category { get; private set; }

    public ThirdPartyScriptPlacement Placement { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyList<ThirdPartyScriptTranslation> Translations => _translations.AsReadOnly();

    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    private ThirdPartyScript(
        Guid id, ThirdPartyScriptProvider provider, ThirdPartyScriptCategory category, ThirdPartyScriptPlacement placement, int sortOrder,
        Guid createdByUserId, DateTime createdAtUtc)
        : base(id)
    {
        Provider = provider;
        Category = category;
        Placement = placement;
        SortOrder = sortOrder;
        IsActive = true;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    private ThirdPartyScript()
    {
    }

    public static Result<ThirdPartyScript> Create(
        ThirdPartyScriptProvider provider,
        ThirdPartyScriptCategory category,
        ThirdPartyScriptPlacement placement,
        int sortOrder,
        LanguageCode defaultLanguageCode,
        string? defaultLanguageName,
        string? defaultLanguagePurpose,
        Guid createdByUserId,
        DateTime createdAtUtc)
    {
        var translationResult = ThirdPartyScriptTranslation.Create(defaultLanguageCode, defaultLanguageName, defaultLanguagePurpose);
        if (translationResult.IsFailure)
        {
            return Result.Failure<ThirdPartyScript>(translationResult.Error);
        }

        var script = new ThirdPartyScript(Guid.NewGuid(), provider, category, placement, sortOrder, createdByUserId, createdAtUtc);
        script._translations.Add(translationResult.Value);

        return Result.Success(script);
    }

    public void Update(
        ThirdPartyScriptProvider provider, ThirdPartyScriptCategory category, ThirdPartyScriptPlacement placement, int sortOrder,
        Guid updatedByUserId, DateTime updatedAtUtc)
    {
        Provider = provider;
        Category = category;
        Placement = placement;
        SortOrder = sortOrder;
        Touch(updatedByUserId, updatedAtUtc);
    }

    // Upserts the translation for languageCode - one call per language the caller wants to set.
    public Result SetTranslation(LanguageCode languageCode, string? name, string? purpose, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is not null)
        {
            var updateResult = existing.Update(name, purpose);
            if (updateResult.IsFailure)
            {
                return updateResult;
            }

            Touch(updatedByUserId, updatedAtUtc);
            return Result.Success();
        }

        var createResult = ThirdPartyScriptTranslation.Create(languageCode, name, purpose);
        if (createResult.IsFailure)
        {
            return createResult;
        }

        _translations.Add(createResult.Value);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // The default-language translation can never be removed (Application layer resolves which
    // language is default and passes it in - ThirdPartyScript itself has no SiteLanguage access,
    // mirroring Partner.RemoveTranslation).
    public Result RemoveTranslation(LanguageCode languageCode, LanguageCode defaultLanguageCode, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (languageCode == defaultLanguageCode)
        {
            return Result.Failure(Error.Conflict(
                "ThirdPartyScript.CannotDeleteDefaultTranslation", "The default language's translation cannot be deleted."));
        }

        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is null)
        {
            return Result.Failure(Error.NotFound(
                "ThirdPartyScript.TranslationNotFound", $"No translation exists for language '{languageCode}'."));
        }

        _translations.Remove(existing);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    public void Activate(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        IsActive = true;
        Touch(updatedByUserId, updatedAtUtc);
    }

    public void Deactivate(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        IsActive = false;
        Touch(updatedByUserId, updatedAtUtc);
    }

    private void Touch(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;
        RowVersion = Guid.NewGuid().ToByteArray();
    }
}
