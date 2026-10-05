using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UnarchiveFormSubmission;

public sealed record UnarchiveFormSubmissionCommand(Guid Id, byte[] RowVersion) : IRequest<Result>;
