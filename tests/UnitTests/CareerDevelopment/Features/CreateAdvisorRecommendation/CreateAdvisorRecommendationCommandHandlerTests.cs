using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.CareerDevelopment.Features.CreateAdvisorRecommendation;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.CareerDevelopment.TestDoubles;
using GenclikMerkezi.UnitTests.Matching.TestDoubles;

namespace GenclikMerkezi.UnitTests.CareerDevelopment.Features.CreateAdvisorRecommendation;

public class CreateAdvisorRecommendationCommandHandlerTests
{
    private readonly FakeAdvisorRecommendationRepository _advisorRecommendationRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeCandidateModuleContract _candidateModuleContract = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _advisorUserId = Guid.NewGuid();

    private CreateAdvisorRecommendationCommandHandler CreateHandler() =>
        new(_advisorRecommendationRepository, _careerAdvisorModuleContract, _candidateModuleContract,
            new FakeCurrentUserContext(_advisorUserId), _unitOfWork);

    [Fact]
    public async Task Handle_WithOwnCandidate_CreatesRecommendation_AndSaves()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", advisorId, Guid.NewGuid(), "aday@example.com"));

        var result = await CreateHandler().Handle(
            new CreateAdvisorRecommendationCommand(candidateCvId, "Networking etkinliklerine katılmalı"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var recommendation = Assert.Single(_advisorRecommendationRepository.AdvisorRecommendations);
        Assert.Equal(candidateCvId, recommendation.CandidateCvId);
        Assert.Equal(advisorId, recommendation.AdvisorId);
        Assert.Equal("Networking etkinliklerine katılmalı", recommendation.Content);
        Assert.Equal(recommendation.Id, result.Value.AdvisorRecommendationId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenCallerIsNotACareerAdvisor_ReturnsForbidden()
    {
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = null;

        var result = await CreateHandler().Handle(
            new CreateAdvisorRecommendationCommand(Guid.NewGuid(), "Öneri"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Empty(_advisorRecommendationRepository.AdvisorRecommendations);
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
            new CreateAdvisorRecommendationCommand(candidateCvId, "Öneri"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Empty(_advisorRecommendationRepository.AdvisorRecommendations);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
