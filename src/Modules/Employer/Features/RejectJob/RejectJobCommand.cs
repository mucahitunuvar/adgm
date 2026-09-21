using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.RejectJob;

public sealed record RejectJobCommand(Guid JobId, string Reason) : IRequest<Result>;
