using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Modules.Matching.Domain;
using GenclikMerkezi.Modules.Matching.Features.CreateCandidateSuggestion;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;
using GenclikMerkezi.UnitTests.Matching.TestDoubles;

namespace GenclikMerkezi.UnitTests.Matching.Features.CreateCandidateSuggestion;

public class CreateCandidateSuggestionCommandHandlerTests
{
    private readonly FakeCandidateSuggestionRepository _candidateSuggestionRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeCandidateModuleContract _candidateModuleContract = new();
    private readonly FakePersonnelNeedModuleContract _personnelNeedModuleContract = new();
    private readonly GenclikMerkezi.UnitTests.Candidate.TestDoubles.FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _advisorUserId = Guid.NewGuid();

    private CreateCandidateSuggestionCommandHandler CreateHandler() =>
        new(_candidateSuggestionRepository, _careerAdvisorModuleContract, _candidateModuleContract,
            _personnelNeedModuleContract, new FakeCurrentUserContext(_advisorUserId), _unitOfWork);

    private static PersonnelNeedSummary CreateGeneralPoolSummary(Guid personnelNeedId) =>
        new(personnelNeedId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            3, Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), DateTime.UtcNow);

    [Fact]
    public async Task Handle_WithOwnCandidateAndPersonnelNeedInGeneralPool_CreatesSuggestion_AndSaves()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        var personnelNeedId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", advisorId));
        _personnelNeedModuleContract.SeedGeneralPool(CreateGeneralPoolSummary(personnelNeedId));

        var result = await CreateHandler().Handle(
            new CreateCandidateSuggestionCommand(personnelNeedId, candidateCvId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var suggestion = Assert.Single(_candidateSuggestionRepository.Suggestions);
        Assert.Equal(personnelNeedId, suggestion.PersonnelNeedId);
        Assert.Equal(candidateCvId, suggestion.CandidateCvId);
        Assert.Equal(advisorId, suggestion.SuggestingAdvisorId);
        Assert.Equal(CandidateSuggestionStatus.Onerildi, suggestion.Status);
        Assert.Equal(suggestion.Id, result.Value.CandidateSuggestionId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenCallerIsNotACareerAdvisor_ReturnsForbidden()
    {
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = null;

        var result = await CreateHandler().Handle(
            new CreateCandidateSuggestionCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithCandidateNotOwnedByCaller_ReturnsForbidden()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        var personnelNeedId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", Guid.NewGuid()));
        _personnelNeedModuleContract.SeedGeneralPool(CreateGeneralPoolSummary(personnelNeedId));

        var result = await CreateHandler().Handle(
            new CreateCandidateSuggestionCommand(personnelNeedId, candidateCvId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Empty(_candidateSuggestionRepository.Suggestions);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenPersonnelNeedNotInGeneralPool_ReturnsConflict()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", advisorId));
        // PersonnelNeed hiç seed edilmedi -> IsInGeneralPoolAsync false döner.

        var result = await CreateHandler().Handle(
            new CreateCandidateSuggestionCommand(Guid.NewGuid(), candidateCvId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenAlreadySuggested_ReturnsConflict()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        var personnelNeedId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", advisorId));
        _personnelNeedModuleContract.SeedGeneralPool(CreateGeneralPoolSummary(personnelNeedId));
        _candidateSuggestionRepository.Add(
            GenclikMerkezi.Modules.Matching.Domain.CandidateSuggestion.Create(personnelNeedId, candidateCvId, advisorId, DateTime.UtcNow));

        var result = await CreateHandler().Handle(
            new CreateCandidateSuggestionCommand(personnelNeedId, candidateCvId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
