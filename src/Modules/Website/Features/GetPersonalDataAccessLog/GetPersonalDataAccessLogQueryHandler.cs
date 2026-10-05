using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPersonalDataAccessLog;

public sealed class GetPersonalDataAccessLogQueryHandler(IPersonalDataAccessLogRepository personalDataAccessLogRepository)
    : IRequestHandler<GetPersonalDataAccessLogQuery, Result<PagedResult<PersonalDataAccessLogResponse>>>
{
    public async Task<Result<PagedResult<PersonalDataAccessLogResponse>>> Handle(
        GetPersonalDataAccessLogQuery request, CancellationToken cancellationToken)
    {
        var paged = await personalDataAccessLogRepository.SearchAsync(request.From, request.To, request, cancellationToken);

        var items = paged.Items
            .Select(a => new PersonalDataAccessLogResponse(
                a.Id, a.UserId, a.AccessedAtUtc, a.EntityType.ToString(), a.EntityId, a.Action.ToString(), a.Detail))
            .ToList();

        return Result.Success(new PagedResult<PersonalDataAccessLogResponse>(items, paged.TotalCount, paged.Page, paged.PageSize));
    }
}
