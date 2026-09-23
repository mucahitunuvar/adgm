using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.CareerDevelopment.Domain;
using GenclikMerkezi.Modules.CareerDevelopment.Features.CreateTrainingRecommendation;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.CareerDevelopment.TestDoubles;
using GenclikMerkezi.UnitTests.Matching.TestDoubles;

namespace GenclikMerkezi.UnitTests.CareerDevelopment.Features.CreateTrainingRecommendation;

public class CreateTrainingRecommendationCommandHandlerTests
{
    private readonly FakeTrainingRecommendationRepository _trainingRecommendationRepository = new();
    private readonly FakeDevelopmentPlanRepository _developmentPlanRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeCandidateModuleContract _candidateModuleContract = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _advisorUserId = Guid.NewGuid();

    private CreateTrainingRecommendationCommandHandler CreateHandler() =>
        new(_trainingRecommendationRepository, _developmentPlanRepository, _careerAdvisorModuleContract,
            _candidateModuleContract, new FakeCurrentUserContext(_advisorUserId), _unitOfWork);

    private void SeedOwnCandidate(Guid advisorId, Guid candidateCvId)
    {
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", advisorId, Guid.NewGuid(), "aday@example.com"));
    }

    [Fact]
    public async Task Handle_WithoutDevelopmentPlanId_CreatesRecommendation_AndSaves()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        var trainingId = Guid.NewGuid();
        SeedOwnCandidate(advisorId, candidateCvId);

        var result = await CreateHandler().Handle(
            new CreateTrainingRecommendationCommand(candidateCvId, null, trainingId, "Uygun eğitim"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var recommendation = Assert.Single(_trainingRecommendationRepository.TrainingRecommendations);
        Assert.Equal(candidateCvId, recommendation.CandidateCvId);
        Assert.Null(recommendation.DevelopmentPlanId);
        Assert.Equal(trainingId, recommendation.TrainingId);
        Assert.Equal(advisorId, recommendation.RecommendedByAdvisorId);
        Assert.Equal(recommendation.Id, result.Value.TrainingRecommendationId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithOwnDevelopmentPlan_CreatesRecommendation()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        SeedOwnCandidate(advisorId, candidateCvId);
        var plan = DevelopmentPlan.Create(candidateCvId, null, null, "Açıklama", advisorId, DateTime.UtcNow);
        _developmentPlanRepository.Add(plan);

        var result = await CreateHandler().Handle(
            new CreateTrainingRecommendationCommand(candidateCvId, plan.Id, Guid.NewGuid(), null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(plan.Id, Assert.Single(_trainingRecommendationRepository.TrainingRecommendations).DevelopmentPlanId);
    }

    [Fact]
    public async Task Handle_WhenCallerIsNotACareerAdvisor_ReturnsForbidden()
    {
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = null;

        var result = await CreateHandler().Handle(
            new CreateTrainingRecommendationCommand(Guid.NewGuid(), null, Guid.NewGuid(), null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Empty(_trainingRecommendationRepository.TrainingRecommendations);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithCandidateNotOwnedByCaller_ReturnsForbidden()
    {
        var candidateCvId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = Guid.NewGuid();
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", Guid.NewGuid(), Guid.NewGuid(), "aday@example.com"));

        var result = await CreateHandler().Handle(
            new CreateTrainingRecommendationCommand(candidateCvId, null, Guid.NewGuid(), null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithDevelopmentPlanBelongingToDifferentCandidate_ReturnsConflict()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        SeedOwnCandidate(advisorId, candidateCvId);
        var otherCandidatesPlan = DevelopmentPlan.Create(Guid.NewGuid(), null, null, "Açıklama", advisorId, DateTime.UtcNow);
        _developmentPlanRepository.Add(otherCandidatesPlan);

        var result = await CreateHandler().Handle(
            new CreateTrainingRecommendationCommand(candidateCvId, otherCandidatesPlan.Id, Guid.NewGuid(), null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Empty(_trainingRecommendationRepository.TrainingRecommendations);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownDevelopmentPlanId_ReturnsConflict()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        SeedOwnCandidate(advisorId, candidateCvId);

        var result = await CreateHandler().Handle(
            new CreateTrainingRecommendationCommand(candidateCvId, Guid.NewGuid(), Guid.NewGuid(), null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }
}
