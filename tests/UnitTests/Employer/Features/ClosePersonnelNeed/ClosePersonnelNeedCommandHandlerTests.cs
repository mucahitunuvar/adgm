using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.ClosePersonnelNeed;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.ClosePersonnelNeed;

public class ClosePersonnelNeedCommandHandlerTests
{
    private readonly FakePersonnelNeedRepository _personnelNeedRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private ClosePersonnelNeedCommandHandler CreateHandler() => new(_personnelNeedRepository, _unitOfWork);

    private static PersonnelNeed CreateKendiHavuzundaPersonnelNeed()
    {
        var personnelNeed = PersonnelNeed.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3, Guid.NewGuid(),
            Guid.NewGuid(), null, [], [], [], [], DateTime.UtcNow);
        personnelNeed.Submit();
        return personnelNeed;
    }

    [Fact]
    public async Task Handle_WithKendiHavuzundaPersonnelNeed_ClosesAndSaves()
    {
        var personnelNeed = CreateKendiHavuzundaPersonnelNeed();
        _personnelNeedRepository.Add(personnelNeed);
        var closedByAdvisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();

        var result = await CreateHandler().Handle(
            new ClosePersonnelNeedCommand(personnelNeed.Id, closedByAdvisorId, candidateCvId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(PersonnelNeedStatus.Karsilandi, personnelNeed.Status);
        Assert.Equal(closedByAdvisorId, personnelNeed.ClosedByAdvisorId);
        Assert.Equal(candidateCvId, personnelNeed.FulfilledByCandidateCvId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownPersonnelNeedId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(
            new ClosePersonnelNeedCommand(Guid.NewGuid(), Guid.NewGuid(), null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithPersonnelNeedInTaslak_ReturnsConflict()
    {
        var personnelNeed = PersonnelNeed.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3, Guid.NewGuid(),
            Guid.NewGuid(), null, [], [], [], [], DateTime.UtcNow);
        _personnelNeedRepository.Add(personnelNeed);

        var result = await CreateHandler().Handle(
            new ClosePersonnelNeedCommand(personnelNeed.Id, Guid.NewGuid(), null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
