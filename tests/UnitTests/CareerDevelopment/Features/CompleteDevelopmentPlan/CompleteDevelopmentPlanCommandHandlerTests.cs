using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.CareerDevelopment.Domain;
using GenclikMerkezi.Modules.CareerDevelopment.Features.CompleteDevelopmentPlan;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.CareerDevelopment.TestDoubles;
using GenclikMerkezi.UnitTests.Matching.TestDoubles;

namespace GenclikMerkezi.UnitTests.CareerDevelopment.Features.CompleteDevelopmentPlan;

public class CompleteDevelopmentPlanCommandHandlerTests
{
    private readonly FakeDevelopmentPlanRepository _developmentPlanRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeCandidateModuleContract _candidateModuleContract = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _advisorUserId = Guid.NewGuid();

    private CompleteDevelopmentPlanCommandHandler CreateHandler() =>
        new(_developmentPlanRepository, _careerAdvisorModuleContract, _candidateModuleContract,
            new FakeCurrentUserContext(_advisorUserId), _unitOfWork);

    private DevelopmentPlan CreatePlanForCandidateWithCurrentAdvisor(Guid candidateCvId, Guid currentAdvisorId)
    {
        _candidateModuleContract.Seed(
            new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", currentAdvisorId, Guid.NewGuid(), "aday@example.com"));

        var plan = DevelopmentPlan.Create(candidateCvId, null, null, "Açıklama", currentAdvisorId, DateTime.UtcNow);
        _developmentPlanRepository.Add(plan);

        return plan;
    }

    [Fact]
    public async Task Handle_AsCurrentAdvisor_CompletesPlan_AndSaves()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        var plan = CreatePlanForCandidateWithCurrentAdvisor(candidateCvId, advisorId);

        var result = await CreateHandler().Handle(new CompleteDevelopmentPlanCommand(plan.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(DevelopmentPlanStatus.Tamamlandi, plan.Status);
        Assert.NotNull(plan.CompletedAtUtc);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_AsDifferentAdvisor_ReturnsForbidden_AndDoesNotSave()
    {
        var currentAdvisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = Guid.NewGuid();
        var plan = CreatePlanForCandidateWithCurrentAdvisor(candidateCvId, currentAdvisorId);

        var result = await CreateHandler().Handle(new CompleteDevelopmentPlanCommand(plan.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal(DevelopmentPlanStatus.Aktif, plan.Status);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownDevelopmentPlanId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new CompleteDevelopmentPlanCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithAlreadyCompletedPlan_ReturnsConflict()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        var plan = CreatePlanForCandidateWithCurrentAdvisor(candidateCvId, advisorId);
        plan.Complete(DateTime.UtcNow);

        var result = await CreateHandler().Handle(new CompleteDevelopmentPlanCommand(plan.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
