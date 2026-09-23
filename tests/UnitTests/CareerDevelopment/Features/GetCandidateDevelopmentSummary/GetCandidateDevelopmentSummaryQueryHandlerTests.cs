using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.CareerDevelopment.Domain;
using GenclikMerkezi.Modules.CareerDevelopment.Features.GetCandidateDevelopmentSummary;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.CareerDevelopment.TestDoubles;
using GenclikMerkezi.UnitTests.Matching.TestDoubles;

namespace GenclikMerkezi.UnitTests.CareerDevelopment.Features.GetCandidateDevelopmentSummary;

public class GetCandidateDevelopmentSummaryQueryHandlerTests
{
    private readonly FakeSkillGapRepository _skillGapRepository = new();
    private readonly FakeCareerGoalRepository _careerGoalRepository = new();
    private readonly FakeDevelopmentPlanRepository _developmentPlanRepository = new();
    private readonly FakeTrainingRecommendationRepository _trainingRecommendationRepository = new();
    private readonly FakeAdvisorRecommendationRepository _advisorRecommendationRepository = new();
    private readonly FakeCandidateModuleContract _candidateModuleContract = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly Guid _callerUserId = Guid.NewGuid();

    private GetCandidateDevelopmentSummaryQueryHandler CreateHandler() =>
        new(_skillGapRepository, _careerGoalRepository, _developmentPlanRepository, _trainingRecommendationRepository,
            _advisorRecommendationRepository, _candidateModuleContract, _careerAdvisorModuleContract,
            new FakeCurrentUserContext(_callerUserId));

    [Fact]
    public async Task Handle_AsOwningCandidate_ReturnsAllCollections()
    {
        var candidateCvId = Guid.NewGuid();
        var advisorId = Guid.NewGuid();
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", advisorId, _callerUserId, "aday@example.com"));
        _skillGapRepository.Add(SkillGap.Create(candidateCvId, Guid.NewGuid(), advisorId, null, DateTime.UtcNow));
        _careerGoalRepository.Add(CareerGoal.Create(candidateCvId, "Açıklama", null, advisorId, DateTime.UtcNow));
        _developmentPlanRepository.Add(DevelopmentPlan.Create(candidateCvId, null, null, "Açıklama", advisorId, DateTime.UtcNow));
        _trainingRecommendationRepository.Add(
            TrainingRecommendation.Create(candidateCvId, null, Guid.NewGuid(), advisorId, null, DateTime.UtcNow));
        _advisorRecommendationRepository.Add(AdvisorRecommendation.Create(candidateCvId, advisorId, "Öneri", DateTime.UtcNow));

        var result = await CreateHandler().Handle(new GetCandidateDevelopmentSummaryQuery(candidateCvId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.SkillGaps);
        Assert.Single(result.Value.CareerGoals);
        Assert.Single(result.Value.DevelopmentPlans);
        Assert.Single(result.Value.TrainingRecommendations);
        Assert.Single(result.Value.AdvisorRecommendations);
    }

    [Fact]
    public async Task Handle_AsCurrentAdvisor_ReturnsCollections()
    {
        var candidateCvId = Guid.NewGuid();
        var advisorId = Guid.NewGuid();
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", advisorId, Guid.NewGuid(), "aday@example.com"));
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        _skillGapRepository.Add(SkillGap.Create(candidateCvId, Guid.NewGuid(), advisorId, null, DateTime.UtcNow));

        var result = await CreateHandler().Handle(new GetCandidateDevelopmentSummaryQuery(candidateCvId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.SkillGaps);
    }

    [Fact]
    public async Task Handle_AsUnrelatedCaller_ReturnsForbidden()
    {
        var candidateCvId = Guid.NewGuid();
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", Guid.NewGuid(), Guid.NewGuid(), "aday@example.com"));
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = null;

        var result = await CreateHandler().Handle(new GetCandidateDevelopmentSummaryQuery(candidateCvId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
    }

    [Fact]
    public async Task Handle_AsDifferentAdvisor_ReturnsForbidden()
    {
        var candidateCvId = Guid.NewGuid();
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", Guid.NewGuid(), Guid.NewGuid(), "aday@example.com"));
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = Guid.NewGuid();

        var result = await CreateHandler().Handle(new GetCandidateDevelopmentSummaryQuery(candidateCvId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
    }
}
