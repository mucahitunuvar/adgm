using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.CareerDevelopment.Features.CreateSkillGap;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.CareerDevelopment.TestDoubles;
using GenclikMerkezi.UnitTests.Matching.TestDoubles;

namespace GenclikMerkezi.UnitTests.CareerDevelopment.Features.CreateSkillGap;

public class CreateSkillGapCommandHandlerTests
{
    private readonly FakeSkillGapRepository _skillGapRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeCandidateModuleContract _candidateModuleContract = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _advisorUserId = Guid.NewGuid();

    private CreateSkillGapCommandHandler CreateHandler() =>
        new(_skillGapRepository, _careerAdvisorModuleContract, _candidateModuleContract,
            new FakeCurrentUserContext(_advisorUserId), _unitOfWork);

    [Fact]
    public async Task Handle_WithOwnCandidate_CreatesSkillGap_AndSaves()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        var skillId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        _candidateModuleContract.Seed(new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", advisorId, Guid.NewGuid(), "aday@example.com"));

        var result = await CreateHandler().Handle(
            new CreateSkillGapCommand(candidateCvId, skillId, "İngilizce yetersiz"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var skillGap = Assert.Single(_skillGapRepository.SkillGaps);
        Assert.Equal(candidateCvId, skillGap.CandidateCvId);
        Assert.Equal(skillId, skillGap.SkillId);
        Assert.Equal(advisorId, skillGap.IdentifiedByAdvisorId);
        Assert.Equal("İngilizce yetersiz", skillGap.Notes);
        Assert.Equal(skillGap.Id, result.Value.SkillGapId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenCallerIsNotACareerAdvisor_ReturnsForbidden()
    {
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = null;

        var result = await CreateHandler().Handle(
            new CreateSkillGapCommand(Guid.NewGuid(), Guid.NewGuid(), null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Empty(_skillGapRepository.SkillGaps);
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
            new CreateSkillGapCommand(candidateCvId, Guid.NewGuid(), null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Empty(_skillGapRepository.SkillGaps);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
