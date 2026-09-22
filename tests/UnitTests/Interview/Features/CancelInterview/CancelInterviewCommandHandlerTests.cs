using GenclikMerkezi.Modules.Interview.Domain;
using GenclikMerkezi.Modules.Interview.Features.CancelInterview;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Interview.TestDoubles;

namespace GenclikMerkezi.UnitTests.Interview.Features.CancelInterview;

public class CancelInterviewCommandHandlerTests
{
    private readonly FakeInterviewRepository _interviewRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _advisorUserId = Guid.NewGuid();

    private CancelInterviewCommandHandler CreateHandler() =>
        new(_interviewRepository, _careerAdvisorModuleContract, new FakeCurrentUserContext(_advisorUserId), _unitOfWork);

    private static GenclikMerkezi.Modules.Interview.Domain.Interview CreateInterviewForAdvisor(Guid organizingAdvisorId) =>
        GenclikMerkezi.Modules.Interview.Domain.Interview.Create(
            Guid.NewGuid(), Guid.NewGuid(), organizingAdvisorId, InterviewRequestedByRole.Candidate, DateTime.UtcNow);

    [Fact]
    public async Task Handle_AsOrganizingAdvisor_CancelsInterview_AndSaves()
    {
        var advisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        var interview = CreateInterviewForAdvisor(advisorId);
        _interviewRepository.Add(interview);

        var result = await CreateHandler().Handle(new CancelInterviewCommand(interview.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(InterviewStatus.IptalEdildi, interview.Status);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_AsDifferentAdvisor_ReturnsForbidden_AndDoesNotSave()
    {
        var organizingAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = Guid.NewGuid();
        var interview = CreateInterviewForAdvisor(organizingAdvisorId);
        _interviewRepository.Add(interview);

        var result = await CreateHandler().Handle(new CancelInterviewCommand(interview.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal(InterviewStatus.TalepEdildi, interview.Status);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithInterviewAlreadyCompleted_ReturnsConflict()
    {
        var advisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        var interview = CreateInterviewForAdvisor(advisorId);
        interview.Schedule(DateTime.UtcNow.AddDays(1));
        interview.RecordResult(InterviewResult.Olumlu, null, DateTime.UtcNow);
        _interviewRepository.Add(interview);

        var result = await CreateHandler().Handle(new CancelInterviewCommand(interview.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
