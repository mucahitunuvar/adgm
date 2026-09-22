using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.Employment.Domain;
using GenclikMerkezi.Modules.Employment.Features.EndEmployment;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Employment.TestDoubles;
using GenclikMerkezi.UnitTests.Matching.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employment.Features.EndEmployment;

public class EndEmploymentCommandHandlerTests
{
    private readonly FakeEmploymentRepository _employmentRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeCandidateModuleContract _candidateModuleContract = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _advisorUserId = Guid.NewGuid();

    private EndEmploymentCommandHandler CreateHandler() =>
        new(_employmentRepository, _careerAdvisorModuleContract, _candidateModuleContract,
            new FakeCurrentUserContext(_advisorUserId), _unitOfWork);

    private GenclikMerkezi.Modules.Employment.Domain.Employment CreateEmploymentForCandidateWithCurrentAdvisor(
        Guid candidateCvId, Guid currentAdvisorId, Guid createdByAdvisorId)
    {
        _candidateModuleContract.Seed(
            new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", currentAdvisorId, Guid.NewGuid(), "aday@example.com"));

        var employment = GenclikMerkezi.Modules.Employment.Domain.Employment.Create(
            candidateCvId, Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow, createdByAdvisorId, DateTime.UtcNow);
        _employmentRepository.Add(employment);

        return employment;
    }

    [Fact]
    public async Task Handle_AsCurrentAdvisor_EndsEmployment_AndSaves()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        var employment = CreateEmploymentForCandidateWithCurrentAdvisor(candidateCvId, advisorId, advisorId);
        var endDateUtc = DateTime.UtcNow.AddDays(30);

        var result = await CreateHandler().Handle(
            new EndEmploymentCommand(employment.Id, "Daha iyi bir fırsat buldu", endDateUtc), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(EmploymentStatus.SonaErdi, employment.Status);
        Assert.Equal(endDateUtc, employment.EndDateUtc);
        Assert.Equal("Daha iyi bir fırsat buldu", employment.DepartureReason);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_AsNewCurrentAdvisor_DifferentFromCreator_StillSucceeds()
    {
        // Danışman değişmiş: kayıt eski danışmanla oluşturulmuş ama adayın GÜNCEL danışmanı farklı -
        // kontrol Employment.CreatedByAdvisorId'ye değil, taze çözülen danışmana göre yapılmalı.
        var oldAdvisorId = Guid.NewGuid();
        var newAdvisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = newAdvisorId;
        var employment = CreateEmploymentForCandidateWithCurrentAdvisor(candidateCvId, newAdvisorId, oldAdvisorId);

        var result = await CreateHandler().Handle(
            new EndEmploymentCommand(employment.Id, "Ayrılma nedeni", DateTime.UtcNow), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(EmploymentStatus.SonaErdi, employment.Status);
    }

    [Fact]
    public async Task Handle_AsDifferentAdvisor_ReturnsForbidden_AndDoesNotSave()
    {
        var currentAdvisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = Guid.NewGuid();
        var employment = CreateEmploymentForCandidateWithCurrentAdvisor(candidateCvId, currentAdvisorId, currentAdvisorId);

        var result = await CreateHandler().Handle(
            new EndEmploymentCommand(employment.Id, "Ayrılma nedeni", DateTime.UtcNow), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal(EmploymentStatus.DevamEdiyor, employment.Status);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownEmploymentId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(
            new EndEmploymentCommand(Guid.NewGuid(), "Ayrılma nedeni", DateTime.UtcNow), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithAlreadyEndedEmployment_ReturnsConflict()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        var employment = CreateEmploymentForCandidateWithCurrentAdvisor(candidateCvId, advisorId, advisorId);
        employment.EndEmployment("İlk neden", DateTime.UtcNow);

        var result = await CreateHandler().Handle(
            new EndEmploymentCommand(employment.Id, "İkinci neden", DateTime.UtcNow.AddDays(1)), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
