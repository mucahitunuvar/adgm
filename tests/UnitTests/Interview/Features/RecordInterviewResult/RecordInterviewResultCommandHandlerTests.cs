using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Modules.Interview.Domain;
using GenclikMerkezi.Modules.Interview.Features.RecordInterviewResult;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;
using GenclikMerkezi.UnitTests.Interview.TestDoubles;
using GenclikMerkezi.UnitTests.Matching.TestDoubles;

namespace GenclikMerkezi.UnitTests.Interview.Features.RecordInterviewResult;

public class RecordInterviewResultCommandHandlerTests
{
    private readonly FakeInterviewRepository _interviewRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeCandidateModuleContract _candidateModuleContract = new();
    private readonly FakeCompanyModuleContract _companyModuleContract = new();
    private readonly FakeNotificationModuleContract _notificationModuleContract = new();
    private readonly GenclikMerkezi.UnitTests.Candidate.TestDoubles.FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _advisorUserId = Guid.NewGuid();

    private RecordInterviewResultCommandHandler CreateHandler() =>
        new(_interviewRepository, _careerAdvisorModuleContract, _candidateModuleContract, _companyModuleContract,
            _notificationModuleContract, new FakeCurrentUserContext(_advisorUserId), _unitOfWork);

    private (GenclikMerkezi.Modules.Interview.Domain.Interview Interview, Guid CandidateCvId, Guid CompanyId) CreateScheduledInterviewForAdvisor(
        Guid organizingAdvisorId)
    {
        var candidateCvId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var interview = GenclikMerkezi.Modules.Interview.Domain.Interview.Create(
            candidateCvId, companyId, organizingAdvisorId, InterviewRequestedByRole.Candidate, DateTime.UtcNow);
        interview.Schedule(DateTime.UtcNow.AddDays(1));
        _interviewRepository.Add(interview);

        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", organizingAdvisorId, Guid.NewGuid(), "aday@example.com"));
        _companyModuleContract.Seed(new CompanySummary(companyId, "Acme A.Ş.", organizingAdvisorId, Guid.NewGuid(), "firma@example.com"));

        return (interview, candidateCvId, companyId);
    }

    [Fact]
    public async Task Handle_AsOrganizingAdvisor_RecordsResult_AndNotifiesBothParties()
    {
        var advisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        var (interview, _, _) = CreateScheduledInterviewForAdvisor(advisorId);

        var result = await CreateHandler().Handle(
            new RecordInterviewResultCommand(interview.Id, "Olumlu", "İyi geçti"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(InterviewStatus.Tamamlandi, interview.Status);
        Assert.Equal(InterviewResult.Olumlu, interview.Outcome);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);

        Assert.Equal(2, _notificationModuleContract.SentNotifications.Count);
        Assert.Contains(_notificationModuleContract.SentNotifications, n => n.RecipientEmail == "aday@example.com");
        Assert.Contains(_notificationModuleContract.SentNotifications, n => n.RecipientEmail == "firma@example.com");
    }

    [Fact]
    public async Task Handle_AsDifferentAdvisor_ReturnsForbidden_AndDoesNotSave_OrNotify()
    {
        var organizingAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = Guid.NewGuid();
        var (interview, _, _) = CreateScheduledInterviewForAdvisor(organizingAdvisorId);

        var result = await CreateHandler().Handle(
            new RecordInterviewResultCommand(interview.Id, "Olumlu", null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal(InterviewStatus.Planlandi, interview.Status);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
        Assert.Empty(_notificationModuleContract.SentNotifications);
    }

    [Fact]
    public async Task Handle_WithInterviewNotScheduled_ReturnsConflict()
    {
        var advisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        var interview = GenclikMerkezi.Modules.Interview.Domain.Interview.Create(
            Guid.NewGuid(), Guid.NewGuid(), advisorId, InterviewRequestedByRole.Candidate, DateTime.UtcNow);
        _interviewRepository.Add(interview);

        var result = await CreateHandler().Handle(
            new RecordInterviewResultCommand(interview.Id, "Olumlu", null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
