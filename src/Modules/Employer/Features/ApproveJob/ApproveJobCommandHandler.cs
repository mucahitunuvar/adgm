using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Notification;
using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Features.ApproveJob;

// Job, Company ile aynı modülde olduğu için "firma onaylı mı" ICompanyModuleContract'a değil
// doğrudan ICompanyRepository'ye sorulur (Görev 2 master prompt notu). Ama "çağıran doğru danışman mı"
// kontrolü modül sınırını aşıyor: Company.CareerAdvisorId, CareerAdvisor.Id'dir (Identity.User.Id
// değil - CareerAdvisorModuleContract.GetActiveAdvisorsAsync a.Id seçiyor), bu yüzden
// ICareerAdvisorModuleContract.GetAdvisorIdByUserIdAsync ile çağıranın kendi CareerAdvisorId'si
// çözülmeden currentUserContext.UserId'yi Company.CareerAdvisorId ile karşılaştırmak yanlış olurdu.
public sealed class ApproveJobCommandHandler(
    ICompanyRepository companyRepository,
    IJobRepository jobRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    INotificationModuleContract notificationModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(EmployerModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ApproveJobCommand, Result>
{
    public async Task<Result> Handle(ApproveJobCommand request, CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetByIdAsync(request.JobId, cancellationToken);

        if (job is null)
        {
            return Result.Failure(Error.NotFound("Job.NotFound", "The specified job could not be found."));
        }

        var company = await companyRepository.GetByIdAsync(job.CompanyId, cancellationToken);

        if (company is null)
        {
            return Result.Failure(Error.NotFound("Company.NotFound", "The specified company could not be found."));
        }

        var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        if (callerAdvisorId is null || company.CareerAdvisorId != callerAdvisorId)
        {
            return Result.Failure(
                Error.Forbidden("Job.NotAssignedReviewer", "Only the company's assigned career advisor may review this job."));
        }

        var approveResult = job.Approve(callerAdvisorId.Value, DateTime.UtcNow);

        if (approveResult.IsFailure)
        {
            return approveResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await notificationModuleContract.SendAsync(
            company.UserId,
            company.ContactEmail,
            "İlan Yayınlandı",
            $"\"{job.Title}\" başlıklı ilanınız onaylandı ve yayınlandı.",
            cancellationToken);

        return Result.Success();
    }
}
