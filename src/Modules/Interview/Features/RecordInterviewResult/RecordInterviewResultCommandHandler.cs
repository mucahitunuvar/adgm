using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Contracts.Notification;
using GenclikMerkezi.Modules.Interview.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Interview.Features.RecordInterviewResult;

// ScheduleInterviewCommandHandler'daki reviewer-doğrulamasının aynısı. Commit sonrası hem adaya hem
// firmaya bildirim gider (ADR-022 §3/§6 deseni: commit'ten SONRA senkron bildirim çağrısı) - master
// prompt'un yalnızca bu adım için istediği bildirim (ScheduleInterview'da yok, bkz. plan).
public sealed class RecordInterviewResultCommandHandler(
    IInterviewRepository interviewRepository,
    ICareerAdvisorModuleContract careerAdvisorModuleContract,
    ICandidateModuleContract candidateModuleContract,
    ICompanyModuleContract companyModuleContract,
    INotificationModuleContract notificationModuleContract,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(InterviewModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<RecordInterviewResultCommand, Result>
{
    public async Task<Result> Handle(RecordInterviewResultCommand request, CancellationToken cancellationToken)
    {
        var interview = await interviewRepository.GetByIdAsync(request.InterviewId, cancellationToken);

        if (interview is null)
        {
            return Result.Failure(Error.NotFound("Interview.NotFound", "The specified interview could not be found."));
        }

        var callerAdvisorId = await careerAdvisorModuleContract.GetAdvisorIdByUserIdAsync(
            currentUserContext.UserId!.Value, cancellationToken);

        if (callerAdvisorId is null || callerAdvisorId != interview.OrganizingAdvisorId)
        {
            return Result.Failure(Error.Forbidden(
                "Interview.NotOrganizingAdvisor", "Only the interview's organizing career advisor may record its result."));
        }

        var recordResult = interview.RecordResult(request.Outcome, request.ResultNotes, DateTime.UtcNow);

        if (recordResult.IsFailure)
        {
            return recordResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var candidate = await candidateModuleContract.GetCandidateCvByIdAsync(interview.CandidateCvId, cancellationToken);
        var company = await companyModuleContract.GetCompanyByIdAsync(interview.CompanyId, cancellationToken);

        if (candidate is not null)
        {
            await notificationModuleContract.SendAsync(
                candidate.UserId,
                candidate.Email,
                "Görüşme Sonucu Bildirildi",
                $"\"{company?.Name}\" firmasıyla olan görüşmenizin sonucu: {interview.Outcome}.",
                cancellationToken);
        }

        if (company is not null)
        {
            await notificationModuleContract.SendAsync(
                company.UserId,
                company.ContactEmail,
                "Görüşme Sonucu Bildirildi",
                $"\"{candidate?.FirstName} {candidate?.LastName}\" adayıyla olan görüşmenizin sonucu: {interview.Outcome}.",
                cancellationToken);
        }

        return Result.Success();
    }
}
