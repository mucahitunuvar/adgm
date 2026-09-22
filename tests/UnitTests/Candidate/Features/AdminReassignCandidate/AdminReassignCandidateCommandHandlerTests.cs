using GenclikMerkezi.Contracts.CareerAdvisor;
using GenclikMerkezi.Modules.Candidate.Features.AdminReassignCandidate;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;

namespace GenclikMerkezi.UnitTests.Candidate.Features.AdminReassignCandidate;

public class AdminReassignCandidateCommandHandlerTests
{
    private readonly FakeCandidateCvRepository _candidateCvRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private AdminReassignCandidateCommandHandler CreateHandler() =>
        new(_candidateCvRepository, _careerAdvisorModuleContract, _unitOfWork);

    private static GenclikMerkezi.Modules.Candidate.Domain.CandidateCv CreateCandidateCv(Guid? careerAdvisorId = null) =>
        GenclikMerkezi.Modules.Candidate.Domain.CandidateCv.Create(
            Guid.NewGuid(), "Ahmet", "Yılmaz", "aday@example.com", null, careerAdvisorId);

    [Fact]
    public async Task Handle_WithActiveAdvisor_ReassignsAndSaves()
    {
        var candidateCv = CreateCandidateCv();
        _candidateCvRepository.Add(candidateCv);
        var newAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.ActiveAdvisors = [new ActiveCareerAdvisorSummary(newAdvisorId, Guid.NewGuid(), "advisor@example.com")];

        var result = await CreateHandler().Handle(
            new AdminReassignCandidateCommand(candidateCv.Id, newAdvisorId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(newAdvisorId, candidateCv.CareerAdvisorId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithNull_RemovesAssignment_AndSaves()
    {
        var existingAdvisorId = Guid.NewGuid();
        var candidateCv = CreateCandidateCv(existingAdvisorId);
        _candidateCvRepository.Add(candidateCv);

        var result = await CreateHandler().Handle(
            new AdminReassignCandidateCommand(candidateCv.Id, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(candidateCv.CareerAdvisorId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithInactiveOrUnknownAdvisor_ReturnsNotFound_AndDoesNotSave()
    {
        var candidateCv = CreateCandidateCv();
        _candidateCvRepository.Add(candidateCv);
        _careerAdvisorModuleContract.ActiveAdvisors = [];

        var result = await CreateHandler().Handle(
            new AdminReassignCandidateCommand(candidateCv.Id, Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Null(candidateCv.CareerAdvisorId);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownCandidateCvId_ReturnsNotFound_AndDoesNotSave()
    {
        var result = await CreateHandler().Handle(
            new AdminReassignCandidateCommand(Guid.NewGuid(), null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
