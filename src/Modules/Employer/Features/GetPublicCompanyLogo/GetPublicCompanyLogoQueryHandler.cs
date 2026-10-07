using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.GetPublicCompanyLogo;

// Görev 2 (Employer public jobs master prompt): yalnızca onaylı firma + ShowLogoOnWebsite=true iken
// dosyayı akış olarak döner; aksi halde 404 (neden ayrımı sızdırılmaz - onaysız firma, consent
// verilmemiş firma ve logosu olmayan firma aynı yanıtı döner). Dosya yolu istemciden gelmez - FileKey
// hiçbir API yanıtında yer almaz, yalnızca Company.Logo üzerinden sunucu tarafında çözülür.
public sealed class GetPublicCompanyLogoQueryHandler(ICompanyRepository companyRepository, IFileStorageService fileStorageService)
    : IRequestHandler<GetPublicCompanyLogoQuery, Result<GetPublicCompanyLogoResult>>
{
    private static readonly Error NotFoundError =
        Error.NotFound("Company.LogoNotFound", "No public logo is available for this company.");

    public async Task<Result<GetPublicCompanyLogoResult>> Handle(
        GetPublicCompanyLogoQuery request, CancellationToken cancellationToken)
    {
        var company = await companyRepository.GetByIdAsync(request.CompanyId, cancellationToken);

        if (company is null || company.Status != CompanyStatus.Approved || !company.ShowLogoOnWebsite || company.Logo is null)
        {
            return Result.Failure<GetPublicCompanyLogoResult>(NotFoundError);
        }

        var content = await fileStorageService.ReadAsync(company.Logo.FileKey, cancellationToken);

        if (content is null)
        {
            return Result.Failure<GetPublicCompanyLogoResult>(NotFoundError);
        }

        return Result.Success(new GetPublicCompanyLogoResult(content, company.Logo.ContentType));
    }
}
