using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.RequestJobRevision;

public sealed record RequestJobRevisionCommand(Guid JobId, string Notes) : IRequest<Result>;
