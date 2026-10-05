using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPersonalDataAccessLog;

public sealed record GetPersonalDataAccessLogQuery(DateTime? From, DateTime? To)
    : PagedRequest, IRequest<Result<PagedResult<PersonalDataAccessLogResponse>>>;
