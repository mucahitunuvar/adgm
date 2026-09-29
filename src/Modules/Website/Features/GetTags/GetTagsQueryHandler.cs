using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetTags;

public sealed class GetTagsQueryHandler(IContentTagRepository contentTagRepository)
    : IRequestHandler<GetTagsQuery, Result<PagedResult<TagResponse>>>
{
    public async Task<Result<PagedResult<TagResponse>>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        LanguageCode? languageCode = null;
        if (!string.IsNullOrWhiteSpace(request.LanguageCode))
        {
            var languageCodeResult = LanguageCode.Create(request.LanguageCode);
            if (languageCodeResult.IsFailure)
            {
                return Result.Failure<PagedResult<TagResponse>>(languageCodeResult.Error);
            }

            languageCode = languageCodeResult.Value;
        }

        var paged = await contentTagRepository.SearchAsync(languageCode, request.Search, request, cancellationToken);

        var items = new List<TagResponse>();
        foreach (var tag in paged.Items)
        {
            var usageCount = await contentTagRepository.CountUsagesAsync(tag.Id, cancellationToken);
            items.Add(new TagResponse(tag.Id, tag.LanguageCode.Value, tag.Name, tag.Slug, usageCount, tag.CreatedAtUtc));
        }

        return Result.Success(new PagedResult<TagResponse>(items, paged.TotalCount, paged.Page, paged.PageSize));
    }
}
