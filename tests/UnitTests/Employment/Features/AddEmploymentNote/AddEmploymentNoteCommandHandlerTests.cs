using GenclikMerkezi.Contracts.Candidate;
using GenclikMerkezi.Modules.Employment.Features.AddEmploymentNote;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Employment.TestDoubles;
using GenclikMerkezi.UnitTests.Matching.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employment.Features.AddEmploymentNote;

public class AddEmploymentNoteCommandHandlerTests
{
    private readonly FakeEmploymentRepository _employmentRepository = new();
    private readonly FakeEmploymentNoteRepository _employmentNoteRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly FakeCandidateModuleContract _candidateModuleContract = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _advisorUserId = Guid.NewGuid();

    private AddEmploymentNoteCommandHandler CreateHandler() =>
        new(_employmentRepository, _employmentNoteRepository, _careerAdvisorModuleContract, _candidateModuleContract,
            new FakeCurrentUserContext(_advisorUserId), _unitOfWork);

    private GenclikMerkezi.Modules.Employment.Domain.Employment CreateEmploymentForCandidateWithCurrentAdvisor(
        Guid candidateCvId, Guid currentAdvisorId)
    {
        _candidateModuleContract.Seed(
            new CandidateCvSummary(candidateCvId, "Ahmet", "Yılmaz", currentAdvisorId, Guid.NewGuid(), "aday@example.com"));

        var employment = GenclikMerkezi.Modules.Employment.Domain.Employment.Create(
            candidateCvId, Guid.NewGuid(), Guid.NewGuid(), null, DateTime.UtcNow, currentAdvisorId, DateTime.UtcNow);
        _employmentRepository.Add(employment);

        return employment;
    }

    [Fact]
    public async Task Handle_AsCurrentAdvisor_AddsNote_AndSaves()
    {
        var advisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = advisorId;
        var employment = CreateEmploymentForCandidateWithCurrentAdvisor(candidateCvId, advisorId);

        var result = await CreateHandler().Handle(
            new AddEmploymentNoteCommand(employment.Id, "İşe uyum süreci iyi gidiyor."), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var note = Assert.Single(_employmentNoteRepository.EmploymentNotes);
        Assert.Equal(employment.Id, note.EmploymentId);
        Assert.Equal(advisorId, note.CareerAdvisorId);
        Assert.Equal("İşe uyum süreci iyi gidiyor.", note.Content);
        Assert.Equal(note.Id, result.Value.EmploymentNoteId);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_AsDifferentAdvisor_ReturnsForbidden_AndDoesNotSave()
    {
        var currentAdvisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = Guid.NewGuid();
        var employment = CreateEmploymentForCandidateWithCurrentAdvisor(candidateCvId, currentAdvisorId);

        var result = await CreateHandler().Handle(
            new AddEmploymentNoteCommand(employment.Id, "Not"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Empty(_employmentNoteRepository.EmploymentNotes);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownEmploymentId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(
            new AddEmploymentNoteCommand(Guid.NewGuid(), "Not"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
