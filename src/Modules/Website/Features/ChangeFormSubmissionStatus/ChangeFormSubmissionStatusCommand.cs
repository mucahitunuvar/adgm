using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ChangeFormSubmissionStatus;

public sealed record ChangeFormSubmissionStatusCommand(Guid Id, byte[] RowVersion, string Status) : IRequest<Result>;
