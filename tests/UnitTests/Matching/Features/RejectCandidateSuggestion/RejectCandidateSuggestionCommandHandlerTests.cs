using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Modules.Matching.Domain;
using GenclikMerkezi.Modules.Matching.Features.RejectCandidateSuggestion;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;
using GenclikMerkezi.UnitTests.Matching.TestDoubles;

namespace GenclikMerkezi.UnitTests.Matching.Features.RejectCandidateSuggestion;

public class RejectCandidateSuggestionCommandHandlerTests
{
    private readonly FakeCandidateSuggestionRepository _candidateSuggestionRepository = new();
    private readonly FakePersonnelNeedModuleContract _personnelNeedModuleContract = new();
    private readonly FakeCompanyModuleContract _companyModuleContract = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly GenclikMerkezi.UnitTests.Candidate.TestDoubles.FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _advisorUserId = Guid.NewGuid();

    private RejectCandidateSuggestionCommandHandler CreateHandler() =>
        new(_candidateSuggestionRepository, _personnelNeedModuleContract, _companyModuleContract,
            _careerAdvisorModuleContract, new FakeCurrentUserContext(_advisorUserId), _unitOfWork);

    private Guid SeedPersonnelNeedForAdvisor(Guid careerAdvisorId)
    {
        var companyId = Guid.NewGuid();
        var personnelNeedId = Guid.NewGuid();
        _companyModuleContract.Seed(new CompanySummary(companyId, "Acme A.Ş.", careerAdvisorId, Guid.NewGuid(), "firma@example.com"));
        _personnelNeedModuleContract.Seed(new PersonnelNeedSummary(
            personnelNeedId, companyId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            3, Guid.NewGuid(), Guid.NewGuid(), null, careerAdvisorId, DateTime.UtcNow));
        return personnelNeedId;
    }

    [Fact]
    public async Task Handle_AsAssignedAdvisor_RejectsSuggestion_AndDoesNotTouchPersonnelNeed()
    {
        var careerAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = careerAdvisorId;
        var personnelNeedId = SeedPersonnelNeedForAdvisor(careerAdvisorId);
        var suggestion = CandidateSuggestion.Create(personnelNeedId, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        _candidateSuggestionRepository.Add(suggestion);

        var result = await CreateHandler().Handle(new RejectCandidateSuggestionCommand(suggestion.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CandidateSuggestionStatus.Reddedildi, suggestion.Status);
        Assert.Equal(careerAdvisorId, suggestion.DecidedByAdvisorId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
        // PersonnelNeed'e dokunulmaz - hâlâ GenelHavuzda kalır, başka danışman önerebilir.
        Assert.Null(_personnelNeedModuleContract.CloseCall);
    }

    [Fact]
    public async Task Handle_AsDifferentAdvisor_ReturnsForbidden_AndDoesNotSave()
    {
        var assignedAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = Guid.NewGuid();
        var personnelNeedId = SeedPersonnelNeedForAdvisor(assignedAdvisorId);
        var suggestion = CandidateSuggestion.Create(personnelNeedId, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        _candidateSuggestionRepository.Add(suggestion);

        var result = await CreateHandler().Handle(new RejectCandidateSuggestionCommand(suggestion.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal(CandidateSuggestionStatus.Onerildi, suggestion.Status);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownSuggestionId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new RejectCandidateSuggestionCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
