using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.Interview.Domain;
using GenclikMerkezi.Modules.Interview.Features.GetMyInterviewsAsCandidate;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Interview.TestDoubles;
using GenclikMerkezi.UnitTests.Matching.TestDoubles;

namespace GenclikMerkezi.UnitTests.Interview.Features.GetMyInterviewsAsCandidate;

public class GetMyInterviewsAsCandidateQueryHandlerTests
{
    private readonly FakeInterviewRepository _interviewRepository = new();
    private readonly FakeCandidateModuleContract _candidateModuleContract = new();
    private readonly Guid _candidateUserId = Guid.NewGuid();

    private GetMyInterviewsAsCandidateQueryHandler CreateHandler() =>
        new(_interviewRepository, _candidateModuleContract, new FakeCurrentUserContext(_candidateUserId));

    [Fact]
    public async Task Handle_ReturnsOwnCandidateCvsInterviews()
    {
        var candidateCvId = Guid.NewGuid();
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", Guid.NewGuid(), _candidateUserId, "aday@example.com"));

        var ownInterview = GenclikMerkezi.Modules.Interview.Domain.Interview.Create(
            candidateCvId, Guid.NewGuid(), Guid.NewGuid(), InterviewRequestedByRole.Candidate, DateTime.UtcNow);
        _interviewRepository.Add(ownInterview);

        var otherInterview = GenclikMerkezi.Modules.Interview.Domain.Interview.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), InterviewRequestedByRole.Candidate, DateTime.UtcNow);
        _interviewRepository.Add(otherInterview);

        var result = await CreateHandler().Handle(new GetMyInterviewsAsCandidateQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var returned = Assert.Single(result.Value);
        Assert.Equal(ownInterview.Id, returned.Id);
    }

    [Fact]
    public async Task Handle_WithNoCandidateCvForCurrentUser_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new GetMyInterviewsAsCandidateQuery(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
