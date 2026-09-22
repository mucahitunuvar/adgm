using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employment.Features.EndEmployment;

public sealed record EndEmploymentCommand(Guid EmploymentId, string DepartureReason, DateTime EndDateUtc) : IRequest<Result>;
