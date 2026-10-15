using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentCategoryById;

public sealed class GetContentCategoryByIdQueryHandler(IContentCategoryRepository contentCategoryRepository)
    : IRequestHandler<GetContentCategoryByIdQuery, Result<ContentCategoryDetailResponse>>
{
    public async Task<Result<ContentCategoryDetailResponse>> Handle(GetContentCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await contentCategoryRepository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null || category.ContentTypeId != request.TypeId)
        {
            return Result.Failure<ContentCategoryDetailResponse>(
                Error.NotFound("ContentCategory.NotFound", $"Category '{request.Id}' could not be found."));
        }

        var translations = category.Translations
            .Select(t => new ContentCategoryTranslationResponse(
                t.LanguageCode.Value, t.Name, t.Slug,
                new ContentCategorySeoResponse(
                    t.Seo.MetaTitle, t.Seo.MetaDescription, t.Seo.MetaKeywords, t.Seo.OgTitle, t.Seo.OgDescription,
                    t.Seo.OgImageMediaId, t.Seo.CanonicalUrl, t.Seo.NoIndex)))
            .ToList();

        var response = new ContentCategoryDetailResponse(
            category.Id, category.ContentTypeId, category.ParentId, category.SortOrder, category.IsActive,
            category.RowVersion, translations);

        return Result.Success(response);
    }
}
