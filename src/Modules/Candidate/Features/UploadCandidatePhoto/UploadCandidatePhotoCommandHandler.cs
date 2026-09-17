using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Candidate.Features.UploadCandidatePhoto;

public sealed class UploadCandidatePhotoCommandHandler(
    ICandidateCvRepository candidateCvRepository,
    ICurrentUserContext currentUserContext,
    IFileStorageService fileStorageService,
    [FromKeyedServices(CandidateModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UploadCandidatePhotoCommand, Result<string>>
{
    // ADR-019 first-version limits: photo (JPG, PNG - max 2MB).
    private static readonly FileValidationPolicy PhotoPolicy =
        FileValidationPolicy.Create(["jpg", "jpeg", "png"], ["image/jpeg", "image/png"], maxSizeInBytes: 2 * 1024 * 1024);

    public async Task<Result<string>> Handle(UploadCandidatePhotoCommand request, CancellationToken cancellationToken)
    {
        var candidateCv = await candidateCvRepository.GetByIdAsync(request.CandidateCvId, cancellationToken);

        if (candidateCv is null)
        {
            return Result.Failure<string>(Error.NotFound("CandidateCv.NotFound", "The specified candidate CV could not be found."));
        }

        if (candidateCv.UserId != currentUserContext.UserId)
        {
            return Result.Failure<string>(Error.Forbidden("CandidateCv.NotOwner", "You may only update your own candidate CV."));
        }

        var uploadResult = await fileStorageService.UploadAsync(
            request.Content,
            request.FileName,
            request.ContentType,
            FileCategory.CandidatePhoto,
            "CandidateCv",
            candidateCv.Id,
            PhotoPolicy,
            cancellationToken);

        if (uploadResult.IsFailure)
        {
            return Result.Failure<string>(uploadResult.Error);
        }

        var previousPhoto = candidateCv.Photo;
        candidateCv.SetPhoto(uploadResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Best-effort cleanup of the replaced file - a failure here would only leak a now-unreferenced
        // file, never leave the candidate's own record in an inconsistent state (the new Photo is
        // already committed above).
        if (previousPhoto is not null)
        {
            await fileStorageService.DeleteAsync(previousPhoto.FileKey, cancellationToken);
        }

        var url = await fileStorageService.GetUrlAsync(uploadResult.Value.FileKey, cancellationToken);

        return Result.Success(url);
    }
}
