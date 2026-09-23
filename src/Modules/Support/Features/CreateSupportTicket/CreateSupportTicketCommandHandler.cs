using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Contracts.Notification;
using GenclikMerkezi.Modules.Support.Application.Abstractions;
using GenclikMerkezi.Modules.Support.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Support.Features.CreateSupportTicket;

// GetEmploymentsForCandidateQueryHandler'daki "önce Candidate dene, sonra Employer" deseminin aynısı:
// çağıranın rolü, bir Identity claim'i olarak değil, hangi modülün profil kaydına sahip olduğuna göre
// çözülür (endpoint zaten Candidate/Employer rolüyle sınırlı - bu sadece hangisi olduğunu ayırt eder).
public sealed class CreateSupportTicketCommandHandler(
    ISupportTicketRepository supportTicketRepository,
    ICandidateModuleContract candidateModuleContract,
    ICompanyModuleContract companyModuleContract,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    IIdentityService identityService,
    INotificationModuleContract notificationModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(SupportModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSupportTicketCommand, Result<CreateSupportTicketResponse>>
{
    public async Task<Result<CreateSupportTicketResponse>> Handle(
        CreateSupportTicketCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserContext.UserId!.Value;

        var candidate = await candidateModuleContract.GetCandidateCvByUserIdAsync(userId, cancellationToken);

        SupportTicketOpenerRole openedByRole;
        Guid? candidateCvId = null;
        Guid? companyId = null;
        Guid? ownerCareerAdvisorId;

        if (candidate is not null)
        {
            openedByRole = SupportTicketOpenerRole.Candidate;
            candidateCvId = candidate.Id;
            ownerCareerAdvisorId = candidate.CareerAdvisorId;
        }
        else
        {
            var company = await companyModuleContract.GetCompanyByUserIdAsync(userId, cancellationToken);

            if (company is null)
            {
                return Result.Failure<CreateSupportTicketResponse>(Error.Forbidden(
                    "SupportTicket.NoProfile", "Only a candidate or an employer may open a support ticket."));
            }

            openedByRole = SupportTicketOpenerRole.Employer;
            companyId = company.Id;
            ownerCareerAdvisorId = company.CareerAdvisorId;
        }

        // Hiç aktif danışman yoksa (ya da CareerAdvisorId hiç atanmamışsa) AssignedToUserId null kalır
        // - admin manuel üstlenir (task tanımı).
        Guid? assignedToUserId = null;

        if (ownerCareerAdvisorId is not null)
        {
            var activeAdvisors = await careerAdvisorModuleContract.GetActiveAdvisorsAsync(cancellationToken);
            assignedToUserId = activeAdvisors.FirstOrDefault(a => a.CareerAdvisorId == ownerCareerAdvisorId.Value)?.UserId;
        }

        var priority = Enum.Parse<SupportTicketPriority>(request.Priority, ignoreCase: true);

        var ticket = SupportTicket.Create(
            openedByRole, userId, candidateCvId, companyId, request.Subject, priority, assignedToUserId, DateTime.UtcNow);

        supportTicketRepository.Add(ticket);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var openerProfile = await identityService.GetUserProfileAsync(userId, cancellationToken);

        if (openerProfile is not null)
        {
            await notificationModuleContract.SendAsync(
                openerProfile.UserId,
                openerProfile.Email,
                "Destek Talebiniz Alındı",
                $"\"{ticket.Subject}\" konulu destek talebiniz alındı.",
                cancellationToken);
        }

        return Result.Success(new CreateSupportTicketResponse(ticket.Id));
    }
}
