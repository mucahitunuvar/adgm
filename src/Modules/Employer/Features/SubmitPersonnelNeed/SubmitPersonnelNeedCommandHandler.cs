using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Notification;
using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Features.SubmitPersonnelNeed;

// Kullanıcıyla netleştirilen akış (Görev 2): firma gönderince atanmış danışmana bildirim gider.
// Ayrı bir GetAdvisorContactAsync contract metodu eklemek yerine (tasarım kararı - bkz. plan),
// zaten genişletilmiş GetActiveAdvisorsAsync()'i çağırıp Company.CareerAdvisorId ile filtreliyoruz -
// contract'ı "deliberately minimal" tutma ilkesiyle tutarlı.
public sealed class SubmitPersonnelNeedCommandHandler(
    ICompanyRepository companyRepository,
    IPersonnelNeedRepository personnelNeedRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    INotificationModuleContract notificationModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(EmployerModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<SubmitPersonnelNeedCommand, Result>
{
    public async Task<Result> Handle(SubmitPersonnelNeedCommand request, CancellationToken cancellationToken)
    {
        var company = await companyRepository.GetByUserIdAsync(currentUserContext.UserId!.Value, cancellationToken);

        if (company is null)
        {
            return Result.Failure(Error.NotFound("Company.NotFound", "No company was found for the current user."));
        }

        var personnelNeed = await personnelNeedRepository.GetByIdAsync(request.PersonnelNeedId, cancellationToken);

        if (personnelNeed is null)
        {
            return Result.Failure(Error.NotFound("PersonnelNeed.NotFound", "The specified personnel need could not be found."));
        }

        if (personnelNeed.CompanyId != company.Id)
        {
            return Result.Failure(Error.Forbidden("PersonnelNeed.NotOwner", "You may only submit your own company's personnel needs."));
        }

        var submitResult = personnelNeed.Submit();

        if (submitResult.IsFailure)
        {
            return submitResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (company.CareerAdvisorId is not null)
        {
            var activeAdvisors = await careerAdvisorModuleContract.GetActiveAdvisorsAsync(cancellationToken);
            var assignedAdvisor = activeAdvisors.FirstOrDefault(a => a.CareerAdvisorId == company.CareerAdvisorId);

            if (assignedAdvisor is not null)
            {
                await notificationModuleContract.SendAsync(
                    assignedAdvisor.UserId,
                    assignedAdvisor.Email,
                    "Yeni Personel İhtiyacı",
                    $"\"{company.Name}\" firması yeni bir personel ihtiyacı bildirdi.",
                    cancellationToken);
            }
        }

        return Result.Success();
    }
}
