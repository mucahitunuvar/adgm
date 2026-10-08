using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentItemRevisions;

public sealed class GetContentItemRevisionsQueryHandler(
    IContentItemRepository contentItemRepository, IContentItemRevisionRepository contentItemRevisionRepository)
    : IRequestHandler<GetContentItemRevisionsQuery, Result<PagedResult<ContentItemRevisionSummaryResponse>>>
{
    public async Task<Result<PagedResult<ContentItemRevisionSummaryResponse>>> Handle(
        GetContentItemRevisionsQuery request, CancellationToken cancellationToken)
    {
        var contentItem = await contentItemRepository.GetByIdAsync(request.ContentItemId, cancellationToken);
        if (contentItem is null)
        {
            return Result.Failure<PagedResult<ContentItemRevisionSummaryResponse>>(
                Error.NotFound("ContentItem.NotFound", $"Content item '{request.ContentItemId}' could not be found."));
        }

        var paged = await contentItemRevisionRepository.SearchAsync(request.ContentItemId, request, cancellationToken);

        var items = paged.Items
            .Select(r => new ContentItemRevisionSummaryResponse(
                r.RevisionNumber, r.SavedAtUtc, r.SavedByUserId, r.Kind.ToString(), r.ChangedLanguages, r.IsPublishedSnapshot))
            .ToList();

        return Result.Success(new PagedResult<ContentItemRevisionSummaryResponse>(items, paged.TotalCount, paged.Page, paged.PageSize));
    }
}
