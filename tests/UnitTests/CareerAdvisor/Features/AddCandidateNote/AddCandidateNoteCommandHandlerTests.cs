using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Modules.CareerAdvisor.Domain;
using GenclikMerkezi.Modules.CareerAdvisor.Features.AddCandidateNote;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;

namespace GenclikMerkezi.UnitTests.CareerAdvisor.Features.AddCandidateNote;

public class AddCandidateNoteCommandHandlerTests
{
    private readonly FakeCareerAdvisorRepository _careerAdvisorRepository = new();
    private readonly FakeCandidateNoteRepository _candidateNoteRepository = new();
    private readonly FakeIdentityService _identityService = new();
    private readonly FakeNotificationModuleContract _notificationModuleContract = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private AddCandidateNoteCommandHandler CreateHandler(Guid? currentUserId) =>
        new(
            new FakeCurrentUserContext(currentUserId),
            _careerAdvisorRepository,
            _candidateNoteRepository,
            _identityService,
            _notificationModuleContract,
            _unitOfWork);

    private GenclikMerkezi.Modules.CareerAdvisor.Domain.CareerAdvisor SeedCareerAdvisor(Guid userId)
    {
        var careerAdvisor = GenclikMerkezi.Modules.CareerAdvisor.Domain.CareerAdvisor.Create(
            userId, "Ayşe", "Kaya", "danisman@example.com", null, DateTime.UtcNow);
        _careerAdvisorRepository.Add(careerAdvisor);
        return careerAdvisor;
    }

    [Fact]
    public async Task Handle_WhenCurrentUserIsNotACareerAdvisor_ReturnsForbidden()
    {
        var command = new AddCandidateNoteCommand(Guid.NewGuid(), Guid.NewGuid(), nameof(NoteType.Genel), "İçerik");

        var result = await CreateHandler(Guid.NewGuid()).Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Empty(_candidateNoteRepository.CandidateNotes);
    }

    [Fact]
    public async Task Handle_WithGenelNote_SavesNote_AndDoesNotNotify()
    {
        var advisorUserId = Guid.NewGuid();
        var careerAdvisor = SeedCareerAdvisor(advisorUserId);
        var candidateCvId = Guid.NewGuid();
        var candidateUserId = Guid.NewGuid();
        var command = new AddCandidateNoteCommand(candidateCvId, candidateUserId, nameof(NoteType.Genel), "Genel bir not");

        var result = await CreateHandler(advisorUserId).Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var note = Assert.Single(_candidateNoteRepository.CandidateNotes);
        Assert.Equal(candidateCvId, note.CandidateCvId);
        Assert.Equal(careerAdvisor.Id, note.CareerAdvisorId);
        Assert.Equal(NoteType.Genel, note.NoteType);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
        Assert.Empty(_notificationModuleContract.SentNotifications);
    }

    [Fact]
    public async Task Handle_WithIsGorusmesiNote_SendsNotification_WithCandidateProfileDetails()
    {
        var advisorUserId = Guid.NewGuid();
        SeedCareerAdvisor(advisorUserId);
        var candidateUserId = Guid.NewGuid();
        _identityService.UserProfileResult = new IdentityUserProfile(candidateUserId, "aday@example.com", "Ahmet", "Yılmaz", null);
        var command = new AddCandidateNoteCommand(Guid.NewGuid(), candidateUserId, nameof(NoteType.IsGorusmesi), "X firmasıyla 10:00'da görüşme");

        var result = await CreateHandler(advisorUserId).Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var notification = Assert.Single(_notificationModuleContract.SentNotifications);
        Assert.Equal(candidateUserId, notification.UserId);
        Assert.Equal("aday@example.com", notification.RecipientEmail);
        Assert.Contains("Ahmet", notification.Message);
        Assert.Contains("Yılmaz", notification.Message);
        Assert.Contains("X firmasıyla 10:00'da görüşme", notification.Message);
    }

    [Fact]
    public async Task Handle_WithIsGorusmesiNote_WhenCandidateProfileNotFound_StillSavesNote_AndSkipsNotification()
    {
        var advisorUserId = Guid.NewGuid();
        SeedCareerAdvisor(advisorUserId);
        _identityService.UserProfileResult = null;
        var command = new AddCandidateNoteCommand(Guid.NewGuid(), Guid.NewGuid(), nameof(NoteType.IsGorusmesi), "İçerik");

        var result = await CreateHandler(advisorUserId).Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(_candidateNoteRepository.CandidateNotes);
        Assert.Empty(_notificationModuleContract.SentNotifications);
    }
}
