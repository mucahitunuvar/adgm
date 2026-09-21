using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Notification;
using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Features.PoolPersonnelNeed;

// ApproveJobCommandHandler'daki gerekçeyle aynı: reviewer doğrulaması ICareerAdvisorModuleContract
// üzerinden yapılır (Company.CareerAdvisorId, Identity.User.Id değil CareerAdvisor.Id'dir). Commit
// sonrası broadcast (ADR-022 §5 madde 2), işlemi yapan danışman hariç tüm aktif danışmanlara
// (tasarım kararı - bkz. plan: kendine bildirim göndermek anlamsız gürültü olurdu).
public sealed class PoolPersonnelNeedCommandHandler(
    ICompanyRepository companyRepository,
    IPersonnelNeedRepository personnelNeedRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    INotificationModuleContract notificationModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(EmployerModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<PoolPersonnelNeedCommand, Result>
{
    public async Task<Result> Handle(PoolPersonnelNeedCommand request, CancellationToken cancellationToken)
    {
        var personnelNeed = await personnelNeedRepository.GetByIdAsync(request.PersonnelNeedId, cancellationToken);

        if (personnelNeed is null)
        {
            return Result.Failure(Error.NotFound("PersonnelNeed.NotFound", "The specified personnel need could not be found."));
        }

        var company = await companyRepository.GetByIdAsync(personnelNeed.CompanyId, cancellationToken);

        if (company is null)
        {
            return Result.Failure(Error.NotFound("Company.NotFound", "The specified company could not be found."));
        }

        var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        if (callerAdvisorId is null || company.CareerAdvisorId != callerAdvisorId)
        {
            return Result.Failure(Error.Forbidden(
                "PersonnelNeed.NotAssignedAdvisor", "Only the company's assigned career advisor may pool this personnel need."));
        }

        var poolResult = personnelNeed.PoolToGeneral(callerAdvisorId.Value, DateTime.UtcNow);

        if (poolResult.IsFailure)
        {
            return poolResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var activeAdvisors = await careerAdvisorModuleContract.GetActiveAdvisorsAsync(cancellationToken);
        var recipients = activeAdvisors
            .Where(a => a.CareerAdvisorId != callerAdvisorId.Value)
            .Select(a => new NotificationRecipient(a.UserId, a.Email))
            .ToList();

        if (recipients.Count > 0)
        {
            await notificationModuleContract.SendBulkAsync(
                recipients,
                "Genel Havuza Yeni İhtiyaç",
                $"\"{company.Name}\" firmasının personel ihtiyacı Genel Havuz'a atandı.",
                cancellationToken);
        }

        return Result.Success();
    }
}
