using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.GetCandidateCv;

public sealed record GetCandidateCvQuery(Guid CandidateCvId) : IRequest<Result<GetCandidateCvResponse>>;
