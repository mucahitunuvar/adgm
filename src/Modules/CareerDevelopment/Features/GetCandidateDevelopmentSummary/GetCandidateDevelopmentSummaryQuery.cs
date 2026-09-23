using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.GetCandidateDevelopmentSummary;

public sealed record GetCandidateDevelopmentSummaryQuery(Guid CandidateCvId) : IRequest<Result<GetCandidateDevelopmentSummaryResponse>>;
