using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Candidate.Features.UploadCandidateCvFile;

public sealed class UploadCandidateCvFileCommandHandler(
    ICandidateCvRepository candidateCvRepository,
    ICandidateCvContentRepository candidateCvContentRepository,
    ICurrentUserContext currentUserContext,
    IFileStorageService fileStorageService,
    [FromKeyedServices(CandidateModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UploadCandidateCvFileCommand, Result<string>>
{
    // ADR-019 first-version limits: CV file (PDF, DOCX - max 5MB).
    private static readonly FileValidationPolicy CvFilePolicy = FileValidationPolicy.Create(
        ["pdf", "docx"],
        ["application/pdf", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
        maxSizeInBytes: 5 * 1024 * 1024);

    public async Task<Result<string>> Handle(UploadCandidateCvFileCommand request, CancellationToken cancellationToken)
    {
        var candidateCv = await candidateCvRepository.GetByIdAsync(request.CandidateCvId, cancellationToken);

        if (candidateCv is null)
        {
            return Result.Failure<string>(Error.NotFound("CandidateCv.NotFound", "The specified candidate CV could not be found."));
        }

        if (candidateCv.UserId != currentUserContext.UserId)
        {
            return Result.Failure<string>(Error.Forbidden("CandidateCv.NotOwner", "You may only update your own candidate CV content."));
        }

        var content = await candidateCvContentRepository.GetByCandidateCvIdAsync(candidateCv.Id, cancellationToken);

        if (content is null)
        {
            return Result.Failure<string>(
                Error.NotFound("CandidateCvContent.NotFound", "The specified candidate CV content could not be found."));
        }

        var uploadResult = await fileStorageService.UploadAsync(
            request.Content,
            request.FileName,
            request.ContentType,
            "candidate-cv-files",
            "CandidateCvContent",
            content.Id,
            CvFilePolicy,
            cancellationToken);

        if (uploadResult.IsFailure)
        {
            return Result.Failure<string>(uploadResult.Error);
        }

        var previousCvFile = content.CvFile;
        content.SetCvFile(uploadResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (previousCvFile is not null)
        {
            await fileStorageService.DeleteAsync(previousCvFile.FileKey, cancellationToken);
        }

        var url = await fileStorageService.GetUrlAsync(uploadResult.Value.FileKey, cancellationToken);

        return Result.Success(url);
    }
}
