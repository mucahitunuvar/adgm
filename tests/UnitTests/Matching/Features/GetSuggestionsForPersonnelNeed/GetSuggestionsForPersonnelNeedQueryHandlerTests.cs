using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Modules.Matching.Domain;
using GenclikMerkezi.Modules.Matching.Features.GetSuggestionsForPersonnelNeed;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;
using GenclikMerkezi.UnitTests.Matching.TestDoubles;

namespace GenclikMerkezi.UnitTests.Matching.Features.GetSuggestionsForPersonnelNeed;

public class GetSuggestionsForPersonnelNeedQueryHandlerTests
{
    private readonly FakeCandidateSuggestionRepository _candidateSuggestionRepository = new();
    private readonly FakePersonnelNeedModuleContract _personnelNeedModuleContract = new();
    private readonly FakeCompanyModuleContract _companyModuleContract = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeCandidateModuleContract _candidateModuleContract = new();
    private readonly Guid _advisorUserId = Guid.NewGuid();

    private GetSuggestionsForPersonnelNeedQueryHandler CreateHandler() =>
        new(_candidateSuggestionRepository, _personnelNeedModuleContract, _companyModuleContract,
            _careerAdvisorModuleContract, _candidateModuleContract, new FakeCurrentUserContext(_advisorUserId));

    private Guid SeedPersonnelNeedForAdvisor(Guid careerAdvisorId)
    {
        var companyId = Guid.NewGuid();
        var personnelNeedId = Guid.NewGuid();
        _companyModuleContract.Seed(new CompanySummary(companyId, "Acme A.Ş.", careerAdvisorId));
        _personnelNeedModuleContract.Seed(new PersonnelNeedSummary(
            personnelNeedId, companyId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            3, Guid.NewGuid(), Guid.NewGuid(), null, careerAdvisorId, DateTime.UtcNow));
        return personnelNeedId;
    }

    [Fact]
    public async Task Handle_AsAssignedAdvisor_ReturnsSuggestionsWithResolvedCandidateNames()
    {
        var careerAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = careerAdvisorId;
        var personnelNeedId = SeedPersonnelNeedForAdvisor(careerAdvisorId);
        var candidateCvId = Guid.NewGuid();
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", Guid.NewGuid()));
        var suggestion = CandidateSuggestion.Create(personnelNeedId, candidateCvId, Guid.NewGuid(), DateTime.UtcNow);
        _candidateSuggestionRepository.Add(suggestion);

        var result = await CreateHandler().Handle(
            new GetSuggestionsForPersonnelNeedQuery(personnelNeedId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value);
        Assert.Equal(suggestion.Id, item.Id);
        Assert.Equal("Ahmet Yılmaz", item.CandidateName);
        Assert.Equal("Onerildi", item.Status);
    }

    [Fact]
    public async Task Handle_AsDifferentAdvisor_ReturnsForbidden()
    {
        var assignedAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = Guid.NewGuid();
        var personnelNeedId = SeedPersonnelNeedForAdvisor(assignedAdvisorId);

        var result = await CreateHandler().Handle(
            new GetSuggestionsForPersonnelNeedQuery(personnelNeedId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithUnknownPersonnelNeedId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(
            new GetSuggestionsForPersonnelNeedQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithMissingCandidate_ReturnsNullCandidateName_RowStillPresent()
    {
        var careerAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = careerAdvisorId;
        var personnelNeedId = SeedPersonnelNeedForAdvisor(careerAdvisorId);
        // Aday bilinçli olarak seed edilmedi (silinmiş/bulunamayan kayıt senaryosu).
        var suggestion = CandidateSuggestion.Create(personnelNeedId, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        _candidateSuggestionRepository.Add(suggestion);

        var result = await CreateHandler().Handle(
            new GetSuggestionsForPersonnelNeedQuery(personnelNeedId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value);
        Assert.Equal(suggestion.Id, item.Id);
        Assert.Null(item.CandidateName);
    }
}
