using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Modules.Matching.Domain;
using GenclikMerkezi.Modules.Matching.Features.AcceptCandidateSuggestion;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;
using GenclikMerkezi.UnitTests.Matching.TestDoubles;

namespace GenclikMerkezi.UnitTests.Matching.Features.AcceptCandidateSuggestion;

public class AcceptCandidateSuggestionCommandHandlerTests
{
    private readonly FakeCandidateSuggestionRepository _candidateSuggestionRepository = new();
    private readonly FakePersonnelNeedModuleContract _personnelNeedModuleContract = new();
    private readonly FakeCompanyModuleContract _companyModuleContract = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly GenclikMerkezi.UnitTests.Candidate.TestDoubles.FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _advisorUserId = Guid.NewGuid();

    private AcceptCandidateSuggestionCommandHandler CreateHandler() =>
        new(_candidateSuggestionRepository, _personnelNeedModuleContract, _companyModuleContract,
            _careerAdvisorModuleContract, new FakeCurrentUserContext(_advisorUserId), _unitOfWork);

    private (Guid PersonnelNeedId, Guid CompanyId) SeedPersonnelNeedForAdvisor(Guid careerAdvisorId)
    {
        var companyId = Guid.NewGuid();
        var personnelNeedId = Guid.NewGuid();
        _companyModuleContract.Seed(new CompanySummary(companyId, "Acme A.Ş.", careerAdvisorId, Guid.NewGuid(), "firma@example.com"));
        _personnelNeedModuleContract.Seed(new PersonnelNeedSummary(
            personnelNeedId, companyId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            3, Guid.NewGuid(), Guid.NewGuid(), null, careerAdvisorId, DateTime.UtcNow));
        return (personnelNeedId, companyId);
    }

    [Fact]
    public async Task Handle_AsAssignedAdvisor_AcceptsSuggestion_RejectsSiblings_AndClosesPersonnelNeed()
    {
        var careerAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = careerAdvisorId;
        var (personnelNeedId, _) = SeedPersonnelNeedForAdvisor(careerAdvisorId);

        var acceptedSuggestion = CandidateSuggestion.Create(personnelNeedId, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        var siblingSuggestion = CandidateSuggestion.Create(personnelNeedId, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        _candidateSuggestionRepository.Add(acceptedSuggestion);
        _candidateSuggestionRepository.Add(siblingSuggestion);

        var result = await CreateHandler().Handle(
            new AcceptCandidateSuggestionCommand(acceptedSuggestion.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CandidateSuggestionStatus.KabulEdildi, acceptedSuggestion.Status);
        Assert.Equal(careerAdvisorId, acceptedSuggestion.DecidedByAdvisorId);
        Assert.Equal(CandidateSuggestionStatus.Reddedildi, siblingSuggestion.Status);
        Assert.Equal(careerAdvisorId, siblingSuggestion.DecidedByAdvisorId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);

        Assert.NotNull(_personnelNeedModuleContract.CloseCall);
        Assert.Equal(personnelNeedId, _personnelNeedModuleContract.CloseCall!.Value.PersonnelNeedId);
        Assert.Equal(careerAdvisorId, _personnelNeedModuleContract.CloseCall.Value.ClosedByAdvisorId);
        Assert.Equal(acceptedSuggestion.CandidateCvId, _personnelNeedModuleContract.CloseCall.Value.FulfilledByCandidateCvId);
    }

    [Fact]
    public async Task Handle_AsDifferentAdvisor_ReturnsForbidden_AndDoesNotSave()
    {
        var assignedAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = Guid.NewGuid();
        var (personnelNeedId, _) = SeedPersonnelNeedForAdvisor(assignedAdvisorId);
        var suggestion = CandidateSuggestion.Create(personnelNeedId, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        _candidateSuggestionRepository.Add(suggestion);

        var result = await CreateHandler().Handle(new AcceptCandidateSuggestionCommand(suggestion.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal(CandidateSuggestionStatus.Onerildi, suggestion.Status);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
        Assert.Null(_personnelNeedModuleContract.CloseCall);
    }

    [Fact]
    public async Task Handle_WithUnknownSuggestionId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new AcceptCandidateSuggestionCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithSuggestionAlreadyDecided_ReturnsConflict()
    {
        var careerAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = careerAdvisorId;
        var (personnelNeedId, _) = SeedPersonnelNeedForAdvisor(careerAdvisorId);
        var suggestion = CandidateSuggestion.Create(personnelNeedId, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        suggestion.Reject(careerAdvisorId, DateTime.UtcNow);
        _candidateSuggestionRepository.Add(suggestion);

        var result = await CreateHandler().Handle(new AcceptCandidateSuggestionCommand(suggestion.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
