using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.AssignFormSubmission;

public sealed record AssignFormSubmissionCommand(Guid Id, byte[] RowVersion, Guid? AssignedToUserId) : IRequest<Result>;
