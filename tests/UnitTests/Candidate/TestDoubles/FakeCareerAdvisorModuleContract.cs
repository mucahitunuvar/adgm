using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Candidate.TestDoubles;

public sealed class FakeCareerAdvisorModuleContract : ICareerAdvisorModuleContract
{
    public IReadOnlyList<ActiveCareerAdvisorSummary> ActiveAdvisors { get; set; } = [];

    public Result<Guid> CreateMeetingRequestResult { get; set; } = Result.Success(Guid.NewGuid());

    public Result ConfirmMeetingRequestResult { get; set; } = Result.Success();

    public Guid? AdvisorIdForCurrentUser { get; set; }

    public (Guid CandidateCvId, Guid CandidateUserId, Guid CareerAdvisorId)? CreateMeetingRequestCall { get; private set; }

    public (Guid MeetingRequestId, Guid CandidateUserId)? ConfirmMeetingRequestCall { get; private set; }

    public Guid? GetAdvisorIdByUserIdCall { get; private set; }

    public (IReadOnlyList<Guid> CandidateUserIds, string Subject, string Message)? SendBulkNotificationCall { get; private set; }

    public Task<IReadOnlyList<ActiveCareerAdvisorSummary>> GetActiveAdvisorsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(ActiveAdvisors);

    public Task<Result<Guid>> CreateMeetingRequestAsync(
        Guid candidateCvId, Guid candidateUserId, Guid careerAdvisorId, CancellationToken cancellationToken = default)
    {
        CreateMeetingRequestCall = (candidateCvId, candidateUserId, careerAdvisorId);
        return Task.FromResult(CreateMeetingRequestResult);
    }

    public Task<Result> ConfirmMeetingRequestAsync(
        Guid meetingRequestId, Guid candidateUserId, CancellationToken cancellationToken = default)
    {
        ConfirmMeetingRequestCall = (meetingRequestId, candidateUserId);
        return Task.FromResult(ConfirmMeetingRequestResult);
    }

    public Task<Guid?> GetAdvisorIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        GetAdvisorIdByUserIdCall = userId;
        return Task.FromResult(AdvisorIdForCurrentUser);
    }

    public Task SendBulkNotificationAsync(
        IReadOnlyList<Guid> candidateUserIds, string subject, string message, CancellationToken cancellationToken = default)
    {
        SendBulkNotificationCall = (candidateUserIds, subject, message);
        return Task.CompletedTask;
    }
}
