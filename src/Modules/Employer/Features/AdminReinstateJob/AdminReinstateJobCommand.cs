using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.AdminReinstateJob;

public sealed record AdminReinstateJobCommand(Guid JobId) : IRequest<Result>;
