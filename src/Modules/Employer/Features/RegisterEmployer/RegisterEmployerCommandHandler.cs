using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Features.RegisterEmployer;

// Kayıt orkestrasyonu Candidate'daki RegisterCandidateCommandHandler ile birebir aynı iskelet
// (ADR-018 Decision 2 / ADR-023): Identity.User + CareerAdvisor ataması + Company oluşturma tek bir
// handler'da, dağıtık transaction olmadığı için Company oluşturma başarısız olursa User compensation
// olarak deaktive edilir.
public sealed class RegisterEmployerCommandHandler(
    IIdentityService identityService,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICompanyRepository companyRepository,
    [FromKeyedServices(EmployerModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterEmployerCommand, Result<RegisterEmployerResponse>>
{
    private const string EmployerRole = "Employer";

    public async Task<Result<RegisterEmployerResponse>> Handle(
        RegisterEmployerCommand request, CancellationToken cancellationToken)
    {
        var createUserResult = await identityService.CreateUserAsync(
            request.Email, request.Password, request.ContactFirstName, request.ContactLastName,
            request.ContactPhone, EmployerRole, cancellationToken);

        if (createUserResult.IsFailure)
        {
            return Result.Failure<RegisterEmployerResponse>(createUserResult.Error);
        }

        var userId = createUserResult.Value;

        try
        {
            // En-az-yüklü danışman ataması (ADR-023: Admin onayından bağımsız ve önce). Hiç aktif
            // danışman yoksa CareerAdvisorId null kalır ve kayıt yine de başarılı olur - ileriki bir
            // görevdeki ReassignOrphanedCompaniesCommand yeniden atamayı üstlenebilir.
            var activeAdvisors = await careerAdvisorModuleContract.GetActiveAdvisorsAsync(cancellationToken);
            var workloadCounts = await companyRepository.GetCompanyCountsByCareerAdvisorAsync(cancellationToken);
            var chosenCareerAdvisorId = CareerAdvisorAssignmentSelector.SelectLeastLoaded(
                activeAdvisors.Select(a => a.CareerAdvisorId).ToList(), workloadCounts);

            var company = Company.Create(
                userId,
                request.Name,
                request.SectorId,
                request.FoundedYear,
                request.EmployeeCount,
                request.WebsiteUrl,
                request.CountryId,
                request.ProvinceId,
                request.DistrictId,
                request.Address,
                request.AboutHtml,
                request.ContactFirstName,
                request.ContactLastName,
                request.Email,
                request.ContactPhone,
                request.TaxOfficeId,
                request.TaxNumber,
                request.MarketingConsent,
                chosenCareerAdvisorId,
                DateTime.UtcNow);

            companyRepository.Add(company);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new RegisterEmployerResponse(userId, company.Id));
        }
        catch
        {
            // Compensating action: User zaten Identity'nin kendi transaction'ında commit edildi.
            // Company'siz bir hesabı öylece bırakmak yerine deaktive edilir, orijinal hata
            // yeniden fırlatılır (GlobalExceptionHandler bunu 500'e eşler - beklenmeyen bir
            // altyapı hatası, beklenen bir Result hatası değil).
            await identityService.DeactivateUserAsync(userId, cancellationToken);
            throw;
        }
    }
}
