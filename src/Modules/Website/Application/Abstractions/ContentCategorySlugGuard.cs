using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §4.1 (Faz 1b Görev 3): mirrors RoutePrefixGuard's role for ContentType - a category's slug
// must be unique within (ContentTypeId, LanguageCode), checked here once rather than duplicated across
// CreateContentCategory and UpdateContentCategoryTranslation.
public static class ContentCategorySlugGuard
{
    public static async Task<Result> CheckAsync(
        string? slug,
        string? name,
        Guid contentTypeId,
        LanguageCode languageCode,
        Guid? excludeCategoryId,
        IContentCategoryRepository contentCategoryRepository,
        CancellationToken cancellationToken)
    {
        var normalizedResult = Slug.Create(string.IsNullOrWhiteSpace(slug) ? name : slug);
        if (normalizedResult.IsFailure)
        {
            return Result.Failure(normalizedResult.Error);
        }

        var exists = await contentCategoryRepository.SlugExistsAsync(
            contentTypeId, languageCode, normalizedResult.Value.Value, excludeCategoryId, cancellationToken);

        return exists
            ? Result.Failure(Error.Conflict(
                "ContentCategory.SlugAlreadyExists",
                $"'{normalizedResult.Value.Value}' is already used by another category of this content type in language '{languageCode}'."))
            : Result.Success();
    }
}
