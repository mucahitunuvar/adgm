using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DownloadFormSubmissionFile;

public sealed class DownloadFormSubmissionFileQueryHandler(IFormSubmissionRepository formSubmissionRepository, IFileStorageService fileStorageService)
    : IRequestHandler<DownloadFormSubmissionFileQuery, Result<DownloadFormSubmissionFileResult>>
{
    private static readonly Error NotFoundError = Error.NotFound("FormSubmissionFile.NotFound", "This file could not be found.");

    public async Task<Result<DownloadFormSubmissionFileResult>> Handle(
        DownloadFormSubmissionFileQuery request, CancellationToken cancellationToken)
    {
        var submission = await formSubmissionRepository.GetByIdAsync(request.SubmissionId, cancellationToken);
        if (submission is null)
        {
            return Result.Failure<DownloadFormSubmissionFileResult>(NotFoundError);
        }

        var attachment = submission.FileAttachments.FirstOrDefault(f => f.Id == request.FileId);
        if (attachment is null)
        {
            return Result.Failure<DownloadFormSubmissionFileResult>(NotFoundError);
        }

        var content = await fileStorageService.ReadAsync(attachment.File.FileKey, cancellationToken);
        if (content is null)
        {
            return Result.Failure<DownloadFormSubmissionFileResult>(NotFoundError);
        }

        return Result.Success(new DownloadFormSubmissionFileResult(content, attachment.File.ContentType, attachment.File.OriginalFileName));
    }
}
