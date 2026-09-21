using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.ApproveJob;

public sealed record ApproveJobCommand(Guid JobId) : IRequest<Result>;
