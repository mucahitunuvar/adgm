using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.RemoveEducation;

public sealed record RemoveEducationCommand(Guid CandidateCvId, Guid EducationId) : IRequest<Result>;
