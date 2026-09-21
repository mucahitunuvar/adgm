using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Features.UploadCompanyLogo;

// UploadCandidatePhotoCommandHandler'ın birebir aynısı (Görev 4 master prompt) - yalnızca
// Candidate/CandidateCv yerine Employer/Company. Company hiçbir zaman domain event fırlatmadığı için
// (Görev 1) Company.SetLogo de (CandidateCv.SetPhoto'nun aksine) event fırlatmaz, ama handler
// seviyesinde bu bir fark yaratmaz - SaveChangesAsync zaten olay yokken de değişikliği commit eder.
public sealed class UploadCompanyLogoCommandHandler(
    ICompanyRepository companyRepository,
    ICurrentUserContext currentUserContext,
    IFileStorageService fileStorageService,
    [FromKeyedServices(EmployerModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UploadCompanyLogoCommand, Result<string>>
{
    // ADR-019 first-version limits: photo (JPG, PNG - max 2MB) - Candidate'daki PhotoPolicy ile birebir aynı.
    private static readonly FileValidationPolicy LogoPolicy =
        FileValidationPolicy.Create(["jpg", "jpeg", "png"], ["image/jpeg", "image/png"], maxSizeInBytes: 2 * 1024 * 1024);

    public async Task<Result<string>> Handle(UploadCompanyLogoCommand request, CancellationToken cancellationToken)
    {
        var company = await companyRepository.GetByIdAsync(request.CompanyId, cancellationToken);

        if (company is null)
        {
            return Result.Failure<string>(Error.NotFound("Company.NotFound", "The specified company could not be found."));
        }

        if (company.UserId != currentUserContext.UserId)
        {
            return Result.Failure<string>(Error.Forbidden("Company.NotOwner", "You may only update your own company."));
        }

        var uploadResult = await fileStorageService.UploadAsync(
            request.Content,
            request.FileName,
            request.ContentType,
            FileCategory.EmployerLogo,
            "Company",
            company.Id,
            LogoPolicy,
            cancellationToken);

        if (uploadResult.IsFailure)
        {
            return Result.Failure<string>(uploadResult.Error);
        }

        var previousLogo = company.Logo;
        company.SetLogo(uploadResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Best-effort cleanup of the replaced file - a failure here would only leak a now-unreferenced
        // file, never leave the company's own record in an inconsistent state (the new Logo is
        // already committed above).
        if (previousLogo is not null)
        {
            await fileStorageService.DeleteAsync(previousLogo.FileKey, cancellationToken);
        }

        var url = await fileStorageService.GetUrlAsync(uploadResult.Value.FileKey, cancellationToken);

        return Result.Success(url);
    }
}
