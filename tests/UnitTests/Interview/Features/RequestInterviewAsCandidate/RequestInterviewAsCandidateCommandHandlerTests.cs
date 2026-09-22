using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.Interview.Features.RequestInterviewAsCandidate;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Interview.TestDoubles;
using GenclikMerkezi.UnitTests.Matching.TestDoubles;

namespace GenclikMerkezi.UnitTests.Interview.Features.RequestInterviewAsCandidate;

public class RequestInterviewAsCandidateCommandHandlerTests
{
    private readonly FakeInterviewRepository _interviewRepository = new();
    private readonly FakeCandidateModuleContract _candidateModuleContract = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _candidateUserId = Guid.NewGuid();

    private RequestInterviewAsCandidateCommandHandler CreateHandler() =>
        new(_interviewRepository, _candidateModuleContract, new FakeCurrentUserContext(_candidateUserId), _unitOfWork);

    [Fact]
    public async Task Handle_WithOwnCandidateCvAndAssignedAdvisor_CreatesInterview_AndSaves()
    {
        var candidateCvId = Guid.NewGuid();
        var advisorId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", advisorId, _candidateUserId, "aday@example.com"));

        var result = await CreateHandler().Handle(new RequestInterviewAsCandidateCommand(companyId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var interview = Assert.Single(_interviewRepository.Interviews);
        Assert.Equal(candidateCvId, interview.CandidateCvId);
        Assert.Equal(companyId, interview.CompanyId);
        Assert.Equal(advisorId, interview.OrganizingAdvisorId);
        Assert.Equal(interview.Id, result.Value.InterviewId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithNoCandidateCvForCurrentUser_ReturnsNotFound_AndDoesNotSave()
    {
        var result = await CreateHandler().Handle(new RequestInterviewAsCandidateCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithNoAssignedAdvisor_ReturnsConflict_AndDoesNotSave()
    {
        var candidateCvId = Guid.NewGuid();
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", null, _candidateUserId, "aday@example.com"));

        var result = await CreateHandler().Handle(new RequestInterviewAsCandidateCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
