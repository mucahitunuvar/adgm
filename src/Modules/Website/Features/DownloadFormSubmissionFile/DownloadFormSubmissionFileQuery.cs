using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DownloadFormSubmissionFile;

public sealed record DownloadFormSubmissionFileQuery(Guid SubmissionId, Guid FileId) : IRequest<Result<DownloadFormSubmissionFileResult>>;
