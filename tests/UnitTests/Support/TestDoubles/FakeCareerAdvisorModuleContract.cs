using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Support.TestDoubles;

public sealed class FakeCareerAdvisorModuleContract : ICareerAdvisorModuleContract
{
    public IReadOnlyList<ActiveCareerAdvisorSummary> ActiveAdvisors { get; set; } = [];

    public Guid? AdvisorIdForCurrentUser { get; set; }

    public Task<IReadOnlyList<ActiveCareerAdvisorSummary>> GetActiveAdvisorsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(ActiveAdvisors);

    public Task<Result<Guid>> CreateMeetingRequestAsync(
        Guid candidateCvId, Guid candidateUserId, Guid careerAdvisorId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(Guid.NewGuid()));

    public Task<Result> ConfirmMeetingRequestAsync(
        Guid meetingRequestId, Guid candidateUserId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success());

    public Task<Guid?> GetAdvisorIdByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(AdvisorIdForCurrentUser);

    public Task SendBulkNotificationAsync(
        IReadOnlyList<Guid> candidateUserIds, string subject, string message, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
