using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.RemoveExperience;

public sealed record RemoveExperienceCommand(Guid CandidateCvId, Guid ExperienceId) : IRequest<Result>;
