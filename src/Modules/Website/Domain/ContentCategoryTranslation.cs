using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §4.1/§92 (Faz 1b Görev 3). Mirrors ContentTypeTranslation's shape (name + slug + optional
// listing SEO), one per language.
public sealed class ContentCategoryTranslation : Entity
{
    public const int MaxNameLength = 100;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public string Slug { get; private set; } = string.Empty;

    public SeoMetadata Seo { get; private set; } = SeoMetadata.CreateEmpty();

    private ContentCategoryTranslation(Guid id, LanguageCode languageCode, string name, string slug, SeoMetadata seo)
        : base(id)
    {
        LanguageCode = languageCode;
        Name = name;
        Slug = slug;
        Seo = seo;
    }

    private ContentCategoryTranslation()
    {
    }

    public static Result<ContentCategoryTranslation> Create(LanguageCode languageCode, string? name, string? slug, SeoMetadata seo)
    {
        var nameResult = NormalizeName(name);
        if (nameResult.IsFailure)
        {
            return Result.Failure<ContentCategoryTranslation>(nameResult.Error);
        }

        var slugResult = Domain.Slug.Create(string.IsNullOrWhiteSpace(slug) ? name : slug);
        if (slugResult.IsFailure)
        {
            return Result.Failure<ContentCategoryTranslation>(slugResult.Error);
        }

        return Result.Success(new ContentCategoryTranslation(Guid.NewGuid(), languageCode, nameResult.Value, slugResult.Value.Value, seo));
    }

    internal Result Update(string? name, string? slug, SeoMetadata seo)
    {
        var nameResult = NormalizeName(name);
        if (nameResult.IsFailure)
        {
            return nameResult;
        }

        var slugResult = Domain.Slug.Create(string.IsNullOrWhiteSpace(slug) ? name : slug);
        if (slugResult.IsFailure)
        {
            return slugResult;
        }

        Name = nameResult.Value;
        Slug = slugResult.Value.Value;
        Seo = seo;

        return Result.Success();
    }

    private static Result<string> NormalizeName(string? name)
    {
        var normalized = (name ?? string.Empty).Trim();
        if (normalized.Length == 0 || normalized.Length > MaxNameLength)
        {
            return Result.Failure<string>(Error.Validation(
                "ContentCategoryTranslation.NameInvalid", $"Name is required and must be at most {MaxNameLength} characters."));
        }

        return Result.Success(normalized);
    }
}
