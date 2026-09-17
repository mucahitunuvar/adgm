using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateCvContentSummary;

public sealed record UpdateCandidateCvContentSummaryCommand(
    Guid CandidateCvId, string? Summary, string? ComputerSkills, string? Hobbies) : IRequest<Result>;
