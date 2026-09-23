using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.CareerDevelopment.Features.CreateCareerGoal;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.CareerDevelopment.TestDoubles;
using GenclikMerkezi.UnitTests.Matching.TestDoubles;

namespace GenclikMerkezi.UnitTests.CareerDevelopment.Features.CreateCareerGoal;

public class CreateCareerGoalCommandHandlerTests
{
    private readonly FakeCareerGoalRepository _careerGoalRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeCandidateModuleContract _candidateModuleContract = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _advisorUserId = Guid.NewGuid();

    private CreateCareerGoalCommandHandler CreateHandler() =>
        new(_careerGoalRepository, _careerAdvisorModuleContract, _candidateModuleContract,
            new FakeCurrentUserContext(_advisorUserId), _unitOfWork);

    [Fact]
    public async Task Handle_WithOwnCandidate_CreatesCareerGoal_AndSaves()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        var targetPositionId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", advisorId, Guid.NewGuid(), "aday@example.com"));

        var result = await CreateHandler().Handle(
            new CreateCareerGoalCommand(candidateCvId, "Yazılım mühendisi olmak", targetPositionId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var careerGoal = Assert.Single(_careerGoalRepository.CareerGoals);
        Assert.Equal(candidateCvId, careerGoal.CandidateCvId);
        Assert.Equal("Yazılım mühendisi olmak", careerGoal.Description);
        Assert.Equal(targetPositionId, careerGoal.TargetPositionId);
        Assert.Equal(advisorId, careerGoal.SetByAdvisorId);
        Assert.Equal(careerGoal.Id, result.Value.CareerGoalId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenCallerIsNotACareerAdvisor_ReturnsForbidden()
    {
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = null;

        var result = await CreateHandler().Handle(
            new CreateCareerGoalCommand(Guid.NewGuid(), "Açıklama", null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Empty(_careerGoalRepository.CareerGoals);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithCandidateNotOwnedByCaller_ReturnsForbidden()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", Guid.NewGuid(), Guid.NewGuid(), "aday@example.com"));

        var result = await CreateHandler().Handle(
            new CreateCareerGoalCommand(candidateCvId, "Açıklama", null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Empty(_careerGoalRepository.CareerGoals);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
