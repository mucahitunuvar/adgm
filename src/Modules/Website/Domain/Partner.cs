using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §8.2 (Faz 2 Görev 3). A simple aggregate mirroring Video's own pattern (AGENTS.md §10:
// RowVersion is the same application-managed optimistic-concurrency token every other aggregate
// uses; cross-aggregate invariants such as "the logo media asset exists and is an image" belong to
// the Application layer's MediaImageReferenceGuard, not here).
public sealed class Partner : AggregateRoot
{
    public const int MaxWebsiteUrlLength = 500;

    private readonly List<PartnerTranslation> _translations = [];

    public Guid LogoMediaId { get; private set; }

    public string? WebsiteUrl { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    public IReadOnlyList<PartnerTranslation> Translations => _translations.AsReadOnly();

    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Guid? UpdatedByUserId { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    private Partner(
        Guid id, Guid logoMediaId, string? websiteUrl, int sortOrder, Guid createdByUserId, DateTime createdAtUtc)
        : base(id)
    {
        LogoMediaId = logoMediaId;
        WebsiteUrl = websiteUrl;
        SortOrder = sortOrder;
        IsActive = true;
        CreatedByUserId = createdByUserId;
        CreatedAtUtc = createdAtUtc;
    }

    private Partner()
    {
    }

    public static Result<Partner> Create(
        Guid logoMediaId,
        string? websiteUrl,
        int sortOrder,
        LanguageCode defaultLanguageCode,
        string? defaultLanguageName,
        string? defaultLanguageDescription,
        Guid createdByUserId,
        DateTime createdAtUtc)
    {
        var websiteUrlResult = NormalizeWebsiteUrl(websiteUrl);
        if (websiteUrlResult.IsFailure)
        {
            return Result.Failure<Partner>(websiteUrlResult.Error);
        }

        var translationResult = PartnerTranslation.Create(defaultLanguageCode, defaultLanguageName, defaultLanguageDescription);
        if (translationResult.IsFailure)
        {
            return Result.Failure<Partner>(translationResult.Error);
        }

        var partner = new Partner(Guid.NewGuid(), logoMediaId, websiteUrlResult.Value, sortOrder, createdByUserId, createdAtUtc);
        partner._translations.Add(translationResult.Value);

        return Result.Success(partner);
    }

    public Result Update(Guid logoMediaId, string? websiteUrl, int sortOrder, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var websiteUrlResult = NormalizeWebsiteUrl(websiteUrl);
        if (websiteUrlResult.IsFailure)
        {
            return websiteUrlResult;
        }

        LogoMediaId = logoMediaId;
        WebsiteUrl = websiteUrlResult.Value;
        SortOrder = sortOrder;
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // Upserts the translation for languageCode - one call per language the caller wants to set.
    public Result SetTranslation(LanguageCode languageCode, string? name, string? description, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is not null)
        {
            var updateResult = existing.Update(name, description);
            if (updateResult.IsFailure)
            {
                return updateResult;
            }

            Touch(updatedByUserId, updatedAtUtc);
            return Result.Success();
        }

        var createResult = PartnerTranslation.Create(languageCode, name, description);
        if (createResult.IsFailure)
        {
            return createResult;
        }

        _translations.Add(createResult.Value);
        Touch(updatedByUserId, updatedAtUtc);

        return Result.Success();
    }

    // The default-language translation can never be removed (Application layer resolves which
    // language is default and passes it in - Partner itself has no SiteLanguage access, mirroring
    // Video.RemoveTranslation).
    public Result RemoveTranslation(LanguageCode languageCode, LanguageCode defaultLanguageCode, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        if (languageCode == defaultLanguageCode)
        {
            return Result.Failure(Error.Conflict(
                "Partner.CannotDeleteDefaultTranslation", "The default language's translation cannot be deleted."));
        }

        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is null)
        {
            return Result.Failure(Error.NotFound("Partner.TranslationNotFound", $"No translation exists for language '{languageCode}'."));
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

    private static Result<string?> NormalizeWebsiteUrl(string? websiteUrl)
    {
        var trimmed = websiteUrl?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return Result.Success<string?>(null);
        }

        if (trimmed.Length > MaxWebsiteUrlLength
            || !Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps)
        {
            return Result.Failure<string?>(Error.Validation(
                "Partner.WebsiteUrlInvalid", $"Website URL must be an absolute https URL of at most {MaxWebsiteUrlLength} characters."));
        }

        return Result.Success<string?>(trimmed);
    }

    private void Touch(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;
        RowVersion = Guid.NewGuid().ToByteArray();
    }
}
