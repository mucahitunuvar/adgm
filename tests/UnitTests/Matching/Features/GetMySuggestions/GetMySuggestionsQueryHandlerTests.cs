using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.Matching.Domain;
using GenclikMerkezi.Modules.Matching.Features.GetMySuggestions;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Matching.TestDoubles;

namespace GenclikMerkezi.UnitTests.Matching.Features.GetMySuggestions;

public class GetMySuggestionsQueryHandlerTests
{
    private readonly FakeCandidateSuggestionRepository _candidateSuggestionRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeCandidateModuleContract _candidateModuleContract = new();
    private readonly Guid _advisorUserId = Guid.NewGuid();

    private GetMySuggestionsQueryHandler CreateHandler() =>
        new(_candidateSuggestionRepository, _careerAdvisorModuleContract, _candidateModuleContract,
            new FakeCurrentUserContext(_advisorUserId));

    [Fact]
    public async Task Handle_ReturnsOnlySuggestionsMadeByCaller()
    {
        var callerAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = callerAdvisorId;
        var candidateCvId = Guid.NewGuid();
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", callerAdvisorId, Guid.NewGuid(), "aday@example.com"));

        var ownSuggestion = CandidateSuggestion.Create(Guid.NewGuid(), candidateCvId, callerAdvisorId, DateTime.UtcNow);
        _candidateSuggestionRepository.Add(ownSuggestion);

        var otherSuggestion = CandidateSuggestion.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        _candidateSuggestionRepository.Add(otherSuggestion);

        var result = await CreateHandler().Handle(new GetMySuggestionsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value);
        Assert.Equal(ownSuggestion.Id, item.Id);
        Assert.Equal("Ahmet Yılmaz", item.CandidateName);
    }

    [Fact]
    public async Task Handle_WhenCallerIsNotACareerAdvisor_ReturnsForbidden()
    {
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = null;

        var result = await CreateHandler().Handle(new GetMySuggestionsQuery(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
    }
}
