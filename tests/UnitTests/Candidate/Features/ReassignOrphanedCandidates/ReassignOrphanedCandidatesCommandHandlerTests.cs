using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.Candidate.Features.ReassignOrphanedCandidates;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;

namespace GenclikMerkezi.UnitTests.Candidate.Features.ReassignOrphanedCandidates;

public class ReassignOrphanedCandidatesCommandHandlerTests
{
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeCandidateCvRepository _candidateCvRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private ReassignOrphanedCandidatesCommandHandler CreateHandler() =>
        new(_careerAdvisorModuleContract, _candidateCvRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WithNoOrphanedCandidates_ReturnsZero_AndDoesNotSave()
    {
        var deactivatedAdvisorId = Guid.NewGuid();

        var result = await CreateHandler().Handle(new ReassignOrphanedCandidatesCommand(deactivatedAdvisorId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.ReassignedCount);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithOneActiveAdvisor_ReassignsAllOrphanedCandidatesToIt()
    {
        var deactivatedAdvisorId = Guid.NewGuid();
        var onlyActiveAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.ActiveAdvisors = [new ActiveCareerAdvisorSummary(onlyActiveAdvisorId)];

        var orphan1 = GenclikMerkezi.Modules.Candidate.Domain.CandidateCv.Create(
            Guid.NewGuid(), "A", "A", "a1@example.com", null, deactivatedAdvisorId);
        var orphan2 = GenclikMerkezi.Modules.Candidate.Domain.CandidateCv.Create(
            Guid.NewGuid(), "B", "B", "a2@example.com", null, deactivatedAdvisorId);
        _candidateCvRepository.Add(orphan1);
        _candidateCvRepository.Add(orphan2);

        var result = await CreateHandler().Handle(new ReassignOrphanedCandidatesCommand(deactivatedAdvisorId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.ReassignedCount);
        Assert.Equal(onlyActiveAdvisorId, orphan1.CareerAdvisorId);
        Assert.Equal(onlyActiveAdvisorId, orphan2.CareerAdvisorId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithMultipleOrphanedCandidatesAndTwoActiveAdvisors_DistributesBalancedWithinTheBatch()
    {
        var deactivatedAdvisorId = Guid.NewGuid();
        var advisorAId = Guid.NewGuid();
        var advisorBId = Guid.NewGuid();
        _careerAdvisorModuleContract.ActiveAdvisors =
        [
            new ActiveCareerAdvisorSummary(advisorAId),
            new ActiveCareerAdvisorSummary(advisorBId),
        ];

        var orphans = new[]
        {
            GenclikMerkezi.Modules.Candidate.Domain.CandidateCv.Create(Guid.NewGuid(), "A", "A", "a1@example.com", null, deactivatedAdvisorId),
            GenclikMerkezi.Modules.Candidate.Domain.CandidateCv.Create(Guid.NewGuid(), "B", "B", "a2@example.com", null, deactivatedAdvisorId),
            GenclikMerkezi.Modules.Candidate.Domain.CandidateCv.Create(Guid.NewGuid(), "C", "C", "a3@example.com", null, deactivatedAdvisorId),
        };
        foreach (var orphan in orphans)
        {
            _candidateCvRepository.Add(orphan);
        }

        var result = await CreateHandler().Handle(new ReassignOrphanedCandidatesCommand(deactivatedAdvisorId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.ReassignedCount);

        // Eşitlikte küçük Guid kazanır (CareerAdvisorAssignmentSelector.ThenBy) - 3 aday, 2 danışman:
        // ilk aday küçük Guid'e gider (0-0 eşitlik), ikinci aday diğerine (0-1), üçüncü aday tekrar
        // küçük Guid'e gider (1-1 eşitlik) -> küçük Guid 2, büyük Guid 1 aday alır.
        var (moreLoadedAdvisorId, lessLoadedAdvisorId) = advisorAId.CompareTo(advisorBId) < 0 ? (advisorAId, advisorBId) : (advisorBId, advisorAId);
        Assert.Equal(2, orphans.Count(c => c.CareerAdvisorId == moreLoadedAdvisorId));
        Assert.Equal(1, orphans.Count(c => c.CareerAdvisorId == lessLoadedAdvisorId));
    }

    [Fact]
    public async Task Handle_WithNoActiveAdvisors_LeavesOrphanedCandidatesCareerAdvisorIdNull()
    {
        var deactivatedAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.ActiveAdvisors = [];

        var orphan = GenclikMerkezi.Modules.Candidate.Domain.CandidateCv.Create(
            Guid.NewGuid(), "A", "A", "a1@example.com", null, deactivatedAdvisorId);
        _candidateCvRepository.Add(orphan);

        var result = await CreateHandler().Handle(new ReassignOrphanedCandidatesCommand(deactivatedAdvisorId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.ReassignedCount);
        Assert.Null(orphan.CareerAdvisorId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }
}
