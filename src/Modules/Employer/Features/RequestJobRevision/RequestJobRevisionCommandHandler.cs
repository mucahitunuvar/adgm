using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Notification;
using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Features.RequestJobRevision;

// ApproveJobCommandHandler'daki gerekçeyle aynı: reviewer doğrulaması ICareerAdvisorModuleContract
// üzerinden yapılır (Company.CareerAdvisorId, Identity.User.Id değil CareerAdvisor.Id'dir).
public sealed class RequestJobRevisionCommandHandler(
    ICompanyRepository companyRepository,
    IJobRepository jobRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    INotificationModuleContract notificationModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(EmployerModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<RequestJobRevisionCommand, Result>
{
    public async Task<Result> Handle(RequestJobRevisionCommand request, CancellationToken cancellationToken)
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

        var requestRevisionResult = job.RequestRevision(request.Notes, DateTime.UtcNow);

        if (requestRevisionResult.IsFailure)
        {
            return requestRevisionResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Kullanıcıyla netleştirildi (Görev 2): firma düzeltmeleri yapabilmesi için bildirim alır,
        // düzeltmelerin ardından SubmitJobForReview ile tekrar danışman onayına gönderir.
        await notificationModuleContract.SendAsync(
            company.UserId,
            company.ContactEmail,
            "İlan İçin Düzeltme Talep Edildi",
            $"\"{job.Title}\" başlıklı ilanınız için düzeltme talep edildi. Notlar: {request.Notes}",
            cancellationToken);

        return Result.Success();
    }
}
