using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Notification;
using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Features.RejectJob;

// ApproveJobCommandHandler'daki gerekçeyle aynı: reviewer doğrulaması ICareerAdvisorModuleContract
// üzerinden yapılır (Company.CareerAdvisorId, Identity.User.Id değil CareerAdvisor.Id'dir).
public sealed class RejectJobCommandHandler(
    ICompanyRepository companyRepository,
    IJobRepository jobRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    INotificationModuleContract notificationModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(EmployerModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<RejectJobCommand, Result>
{
    public async Task<Result> Handle(RejectJobCommand request, CancellationToken cancellationToken)
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

        var rejectResult = job.Reject(request.Reason, DateTime.UtcNow);

        if (rejectResult.IsFailure)
        {
            return rejectResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Kullanıcıyla netleştirildi (Görev 2): danışman ilanı reddederse açıklamasıyla birlikte
        // firmaya bildirilir.
        await notificationModuleContract.SendAsync(
            company.UserId,
            company.ContactEmail,
            "İlan Reddedildi",
            $"\"{job.Title}\" başlıklı ilanınız reddedildi. Neden: {request.Reason}",
            cancellationToken);

        return Result.Success();
    }
}
