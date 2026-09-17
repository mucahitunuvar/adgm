using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.RemoveCandidateReference;

public sealed record RemoveCandidateReferenceCommand(Guid CandidateCvId, Guid CandidateReferenceId) : IRequest<Result>;
