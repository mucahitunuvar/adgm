using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.AdminReassignCandidate;

public sealed record AdminReassignCandidateCommand(Guid CandidateCvId, Guid? NewCareerAdvisorId) : IRequest<Result>;
