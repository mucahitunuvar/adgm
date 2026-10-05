using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ArchiveFormSubmission;

public sealed record ArchiveFormSubmissionCommand(Guid Id, byte[] RowVersion) : IRequest<Result>;
