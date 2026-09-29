using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentCategoriesByType;

public sealed class GetContentCategoriesByTypeQueryHandler(
    IContentCategoryRepository contentCategoryRepository, IContentItemRepository contentItemRepository, ISiteLanguageRepository siteLanguageRepository)
    : IRequestHandler<GetContentCategoriesByTypeQuery, Result<IReadOnlyList<ContentCategoryTreeItemResponse>>>
{
    public async Task<Result<IReadOnlyList<ContentCategoryTreeItemResponse>>> Handle(
        GetContentCategoriesByTypeQuery request, CancellationToken cancellationToken)
    {
        var categories = await contentCategoryRepository.GetByContentTypeIdAsync(request.TypeId, cancellationToken);
        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure<IReadOnlyList<ContentCategoryTreeItemResponse>>(
                Error.Failure("ContentCategory.NoDefaultLanguage", "No default site language is configured."));
        }

        var roots = categories.Where(c => c.ParentId is null).OrderBy(c => c.SortOrder).ToList();
        var childrenByParentId = categories.Where(c => c.ParentId is not null).ToLookup(c => c.ParentId!.Value);

        var items = new List<ContentCategoryTreeItemResponse>();
        foreach (var root in roots)
        {
            var children = new List<ContentCategoryTreeItemResponse>();
            foreach (var child in childrenByParentId[root.Id].OrderBy(c => c.SortOrder))
            {
                children.Add(await ToResponseAsync(child, defaultLanguage.Code, [], cancellationToken));
            }

            items.Add(await ToResponseAsync(root, defaultLanguage.Code, children, cancellationToken));
        }

        return Result.Success<IReadOnlyList<ContentCategoryTreeItemResponse>>(items);
    }

    private async Task<ContentCategoryTreeItemResponse> ToResponseAsync(
        ContentCategory category, LanguageCode defaultLanguageCode, IReadOnlyList<ContentCategoryTreeItemResponse> children,
        CancellationToken cancellationToken)
    {
        var translation = category.Translations.FirstOrDefault(t => t.LanguageCode == defaultLanguageCode);
        var assignedContentCount = await contentItemRepository.CountByCategoryIdAsync(category.Id, cancellationToken);

        return new ContentCategoryTreeItemResponse(
            category.Id, translation?.Name ?? string.Empty, translation?.Slug ?? string.Empty, category.SortOrder, category.IsActive,
            category.RowVersion, assignedContentCount, children);
    }
}
