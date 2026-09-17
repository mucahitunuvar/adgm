using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.GetCandidateCvContent;

public sealed record GetCandidateCvContentQuery(Guid CandidateCvId) : IRequest<Result<GetCandidateCvContentResponse>>;
