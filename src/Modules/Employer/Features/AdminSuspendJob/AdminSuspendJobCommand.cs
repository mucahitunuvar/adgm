using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.AdminSuspendJob;

public sealed record AdminSuspendJobCommand(Guid JobId, string Reason) : IRequest<Result>;
