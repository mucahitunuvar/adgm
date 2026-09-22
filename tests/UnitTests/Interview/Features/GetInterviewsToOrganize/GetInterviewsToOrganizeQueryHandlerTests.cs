using GenclikMerkezi.Modules.Interview.Domain;
using GenclikMerkezi.Modules.Interview.Features.GetInterviewsToOrganize;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Interview.TestDoubles;

namespace GenclikMerkezi.UnitTests.Interview.Features.GetInterviewsToOrganize;

public class GetInterviewsToOrganizeQueryHandlerTests
{
    private readonly FakeInterviewRepository _interviewRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly Guid _advisorUserId = Guid.NewGuid();

    private GetInterviewsToOrganizeQueryHandler CreateHandler() =>
        new(_interviewRepository, _careerAdvisorModuleContract, new FakeCurrentUserContext(_advisorUserId));

    [Fact]
    public async Task Handle_ReturnsOnlyPendingInterviewsAssignedToCaller()
    {
        var callerAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = callerAdvisorId;

        var myPendingInterview = GenclikMerkezi.Modules.Interview.Domain.Interview.Create(
            Guid.NewGuid(), Guid.NewGuid(), callerAdvisorId, InterviewRequestedByRole.Candidate, DateTime.UtcNow);
        _interviewRepository.Add(myPendingInterview);

        var myCompletedInterview = GenclikMerkezi.Modules.Interview.Domain.Interview.Create(
            Guid.NewGuid(), Guid.NewGuid(), callerAdvisorId, InterviewRequestedByRole.Candidate, DateTime.UtcNow);
        myCompletedInterview.Schedule(DateTime.UtcNow.AddDays(1));
        myCompletedInterview.RecordResult(InterviewResult.Olumlu, null, DateTime.UtcNow);
        _interviewRepository.Add(myCompletedInterview);

        var otherAdvisorInterview = GenclikMerkezi.Modules.Interview.Domain.Interview.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), InterviewRequestedByRole.Candidate, DateTime.UtcNow);
        _interviewRepository.Add(otherAdvisorInterview);

        var result = await CreateHandler().Handle(new GetInterviewsToOrganizeQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var returned = Assert.Single(result.Value);
        Assert.Equal(myPendingInterview.Id, returned.Id);
    }

    [Fact]
    public async Task Handle_WhenCallerIsNotACareerAdvisor_ReturnsForbidden()
    {
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = null;

        var result = await CreateHandler().Handle(new GetInterviewsToOrganizeQuery(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
    }
}
