using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetNotFoundPaths;

public sealed class GetNotFoundPathsQueryHandler(INotFoundLogRepository notFoundLogRepository)
    : IRequestHandler<GetNotFoundPathsQuery, Result<PagedResult<NotFoundLogResponse>>>
{
    public async Task<Result<PagedResult<NotFoundLogResponse>>> Handle(GetNotFoundPathsQuery request, CancellationToken cancellationToken)
    {
        var paged = await notFoundLogRepository.SearchAsync(request, cancellationToken);

        var items = paged.Items
            .Select(n => new NotFoundLogResponse(n.Id, n.LanguageCode.Value, n.Path, n.HitCount, n.FirstSeenAtUtc, n.LastSeenAtUtc))
            .ToList();

        return Result.Success(new PagedResult<NotFoundLogResponse>(items, paged.TotalCount, paged.Page, paged.PageSize));
    }
}
