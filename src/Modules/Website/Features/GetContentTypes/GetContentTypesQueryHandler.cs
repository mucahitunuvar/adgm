using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentTypes;

public sealed class GetContentTypesQueryHandler(IContentTypeRepository contentTypeRepository, ISiteLanguageRepository siteLanguageRepository)
    : IRequestHandler<GetContentTypesQuery, Result<IReadOnlyList<ContentTypeSummaryResponse>>>
{
    public async Task<Result<IReadOnlyList<ContentTypeSummaryResponse>>> Handle(GetContentTypesQuery request, CancellationToken cancellationToken)
    {
        var contentTypes = await contentTypeRepository.GetAllAsync(cancellationToken);
        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);

        var items = contentTypes
            .Select(ct =>
            {
                var defaultTranslation = defaultLanguage is null
                    ? null
                    : ct.Translations.FirstOrDefault(t => t.LanguageCode == defaultLanguage.Code);

                return new ContentTypeSummaryResponse(
                    ct.Id, ct.Key.Value, defaultTranslation?.Name ?? string.Empty, ct.ListTemplate, ct.DetailTemplate,
                    ct.SortMode.ToString(), ct.IsActive, ct.SortOrder,
                    // ContentItem does not exist until Görev 3, so no content can exist yet - this
                    // becomes a real count once IContentItemRepository is available to query.
                    ContentCount: 0,
                    ct.RowVersion);
            })
            .ToList();

        return Result.Success<IReadOnlyList<ContentTypeSummaryResponse>>(items);
    }
}
