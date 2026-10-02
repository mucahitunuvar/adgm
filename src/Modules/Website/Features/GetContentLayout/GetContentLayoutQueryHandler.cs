using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetContentLayout;

public sealed class GetContentLayoutQueryHandler(IContentItemRepository contentItemRepository, IPageLayoutRepository pageLayoutRepository)
    : IRequestHandler<GetContentLayoutQuery, Result<GetContentLayoutResponse>>
{
    public async Task<Result<GetContentLayoutResponse>> Handle(GetContentLayoutQuery request, CancellationToken cancellationToken)
    {
        var contentItem = await contentItemRepository.GetByIdAsync(request.ContentItemId, cancellationToken);
        if (contentItem is null || contentItem.DeletedAtUtc is not null)
        {
            return Result.Failure<GetContentLayoutResponse>(
                Error.NotFound("ContentItem.NotFound", $"Content item '{request.ContentItemId}' could not be found."));
        }

        var layout = await pageLayoutRepository.GetByContentItemIdAsync(request.ContentItemId, cancellationToken);

        return Result.Success(layout is null ? GetContentLayoutResponse.Empty : GetContentLayoutResponse.FromDomain(layout));
    }
}
