using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.CareerDevelopment.Domain;
using GenclikMerkezi.Modules.CareerDevelopment.Features.CreateDevelopmentPlan;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.CareerDevelopment.TestDoubles;
using GenclikMerkezi.UnitTests.Matching.TestDoubles;

namespace GenclikMerkezi.UnitTests.CareerDevelopment.Features.CreateDevelopmentPlan;

public class CreateDevelopmentPlanCommandHandlerTests
{
    private readonly FakeDevelopmentPlanRepository _developmentPlanRepository = new();
    private readonly FakeSkillGapRepository _skillGapRepository = new();
    private readonly FakeCareerGoalRepository _careerGoalRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeCandidateModuleContract _candidateModuleContract = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _advisorUserId = Guid.NewGuid();

    private CreateDevelopmentPlanCommandHandler CreateHandler() =>
        new(_developmentPlanRepository, _skillGapRepository, _careerGoalRepository, _careerAdvisorModuleContract,
            _candidateModuleContract, new FakeCurrentUserContext(_advisorUserId), _unitOfWork);

    private void SeedOwnCandidate(Guid advisorId, Guid candidateCvId)
    {
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", advisorId, Guid.NewGuid(), "aday@example.com"));
    }

    [Fact]
    public async Task Handle_WithoutSkillGapOrCareerGoal_CreatesPlan_AndSaves()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        SeedOwnCandidate(advisorId, candidateCvId);

        var result = await CreateHandler().Handle(
            new CreateDevelopmentPlanCommand(candidateCvId, null, null, "İngilizce seviyesini artır"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var plan = Assert.Single(_developmentPlanRepository.DevelopmentPlans);
        Assert.Equal(candidateCvId, plan.CandidateCvId);
        Assert.Equal("İngilizce seviyesini artır", plan.Description);
        Assert.Equal(advisorId, plan.CreatedByAdvisorId);
        Assert.Equal(DevelopmentPlanStatus.Aktif, plan.Status);
        Assert.Equal(plan.Id, result.Value.DevelopmentPlanId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithOwnSkillGapAndCareerGoal_CreatesPlan()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        SeedOwnCandidate(advisorId, candidateCvId);
        var skillGap = SkillGap.Create(candidateCvId, Guid.NewGuid(), advisorId, null, DateTime.UtcNow);
        var careerGoal = CareerGoal.Create(candidateCvId, "Açıklama", null, advisorId, DateTime.UtcNow);
        _skillGapRepository.Add(skillGap);
        _careerGoalRepository.Add(careerGoal);

        var result = await CreateHandler().Handle(
            new CreateDevelopmentPlanCommand(candidateCvId, skillGap.Id, careerGoal.Id, "Açıklama"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var plan = Assert.Single(_developmentPlanRepository.DevelopmentPlans);
        Assert.Equal(skillGap.Id, plan.SkillGapId);
        Assert.Equal(careerGoal.Id, plan.CareerGoalId);
    }

    [Fact]
    public async Task Handle_WhenCallerIsNotACareerAdvisor_ReturnsForbidden()
    {
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = null;

        var result = await CreateHandler().Handle(
            new CreateDevelopmentPlanCommand(Guid.NewGuid(), null, null, "Açıklama"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Empty(_developmentPlanRepository.DevelopmentPlans);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithCandidateNotOwnedByCaller_ReturnsForbidden()
    {
        var candidateCvId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = Guid.NewGuid();
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", Guid.NewGuid(), Guid.NewGuid(), "aday@example.com"));

        var result = await CreateHandler().Handle(
            new CreateDevelopmentPlanCommand(candidateCvId, null, null, "Açıklama"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithSkillGapBelongingToDifferentCandidate_ReturnsConflict()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        SeedOwnCandidate(advisorId, candidateCvId);
        var otherCandidatesSkillGap = SkillGap.Create(Guid.NewGuid(), Guid.NewGuid(), advisorId, null, DateTime.UtcNow);
        _skillGapRepository.Add(otherCandidatesSkillGap);

        var result = await CreateHandler().Handle(
            new CreateDevelopmentPlanCommand(candidateCvId, otherCandidatesSkillGap.Id, null, "Açıklama"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Empty(_developmentPlanRepository.DevelopmentPlans);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownSkillGapId_ReturnsConflict()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        SeedOwnCandidate(advisorId, candidateCvId);

        var result = await CreateHandler().Handle(
            new CreateDevelopmentPlanCommand(candidateCvId, Guid.NewGuid(), null, "Açıklama"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithCareerGoalBelongingToDifferentCandidate_ReturnsConflict()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        SeedOwnCandidate(advisorId, candidateCvId);
        var otherCandidatesGoal = CareerGoal.Create(Guid.NewGuid(), "Açıklama", null, advisorId, DateTime.UtcNow);
        _careerGoalRepository.Add(otherCandidatesGoal);

        var result = await CreateHandler().Handle(
            new CreateDevelopmentPlanCommand(candidateCvId, null, otherCandidatesGoal.Id, "Açıklama"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Empty(_developmentPlanRepository.DevelopmentPlans);
    }
}
