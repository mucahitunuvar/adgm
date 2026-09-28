using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentTypeById;

public sealed class GetContentTypeByIdQueryHandler(IContentTypeRepository contentTypeRepository)
    : IRequestHandler<GetContentTypeByIdQuery, Result<ContentTypeDetailResponse>>
{
    public async Task<Result<ContentTypeDetailResponse>> Handle(GetContentTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var contentType = await contentTypeRepository.GetByIdAsync(request.Id, cancellationToken);
        if (contentType is null)
        {
            return Result.Failure<ContentTypeDetailResponse>(
                Error.NotFound("ContentType.NotFound", $"Content type '{request.Id}' could not be found."));
        }

        var translations = contentType.Translations
            .Select(t => new ContentTypeTranslationResponse(
                t.LanguageCode.Value, t.Name, t.RoutePrefix,
                new ContentTypeSeoResponse(
                    t.Seo.MetaTitle, t.Seo.MetaDescription, t.Seo.MetaKeywords, t.Seo.OgTitle, t.Seo.OgDescription,
                    t.Seo.OgImageMediaId, t.Seo.CanonicalUrl, t.Seo.NoIndex)))
            .ToList();

        var response = new ContentTypeDetailResponse(
            contentType.Id, contentType.Key.Value, contentType.ListTemplate, contentType.DetailTemplate,
            contentType.SortMode.ToString(), contentType.IsActive, contentType.SortOrder,
            contentType.SupportsHierarchy, contentType.SupportsCategories, contentType.SupportsTags,
            contentType.SupportsDetailImage, contentType.SupportsGallery, contentType.SupportsVideos,
            contentType.SupportsAttachments, contentType.SupportsEvent, contentType.SupportsBlockLayout,
            contentType.SupportsForm, contentType.SupportsRelatedContent, contentType.HasDetailPage,
            contentType.HasListingPage, contentType.IsSearchable, contentType.RequiresReview,
            contentType.RowVersion, translations);

        return Result.Success(response);
    }
}
