using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.GetPublicCompanyProfile;

// Görev 2 (Employer public jobs master prompt) §1: yalnızca onaylı (Approved) firma; aksi halde 404
// (neden ayrımı sızdırılmaz - PendingApproval/Rejected/Deactivated ile bulunamama aynı yanıtı döner).
public sealed class GetPublicCompanyProfileQueryHandler(
    ICompanyRepository companyRepository,
    IJobRepository jobRepository,
    IReferenceDataLookupReader referenceDataLookupReader,
    IHtmlContentSanitizer htmlContentSanitizer)
    : IRequestHandler<GetPublicCompanyProfileQuery, Result<GetPublicCompanyProfileResponse>>
{
    private static readonly Error NotFoundError =
        Error.NotFound("Company.NotFound", "The specified company could not be found.");

    public async Task<Result<GetPublicCompanyProfileResponse>> Handle(
        GetPublicCompanyProfileQuery request, CancellationToken cancellationToken)
    {
        var company = await companyRepository.GetByIdAsync(request.CompanyId, cancellationToken);

        if (company is null || company.Status != CompanyStatus.Approved)
        {
            return Result.Failure<GetPublicCompanyProfileResponse>(NotFoundError);
        }

        var sectorNames = await referenceDataLookupReader.GetByIdsAsync(
            ReferenceDataLookupType.Sector, [company.SectorId], cancellationToken);
        var provinceNames = await referenceDataLookupReader.GetByIdsAsync(
            ReferenceDataLookupType.Province, [company.ProvinceId], cancellationToken);

        var publishedJobCount = await jobRepository.GetPublishedCountByCompanyIdAsync(company.Id, cancellationToken);

        return Result.Success(new GetPublicCompanyProfileResponse(
            company.Id,
            company.Name,
            sectorNames.FirstOrDefault()?.DisplayName,
            provinceNames.FirstOrDefault()?.DisplayName,
            company.FoundedYear,
            company.EmployeeCount,
            company.WebsiteUrl,
            company.AboutHtml is null ? null : htmlContentSanitizer.Sanitize(company.AboutHtml),
            company.Logo is not null && company.ShowLogoOnWebsite,
            publishedJobCount));
    }
}
