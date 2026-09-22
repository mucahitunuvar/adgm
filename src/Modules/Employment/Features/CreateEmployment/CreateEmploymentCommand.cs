using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employment.Features.CreateEmployment;

public sealed record CreateEmploymentCommand(
    Guid CandidateCvId, Guid CompanyId, Guid PositionId, Guid? InterviewId, DateTime StartDateUtc)
    : IRequest<Result<CreateEmploymentResponse>>;
