using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Modules.CareerAdvisor.Features.RejectMeetingRequest;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;

namespace GenclikMerkezi.UnitTests.CareerAdvisor.Features.RejectMeetingRequest;

public class RejectMeetingRequestCommandHandlerTests
{
    private readonly FakeCareerAdvisorRepository _careerAdvisorRepository = new();
    private readonly FakeMeetingRequestRepository _meetingRequestRepository = new();
    private readonly FakeIdentityService _identityService = new();
    private readonly FakeNotificationModuleContract _notificationModuleContract = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private RejectMeetingRequestCommandHandler CreateHandler(Guid? currentUserId) =>
        new(
            new FakeCurrentUserContext(currentUserId),
            _careerAdvisorRepository,
            _meetingRequestRepository,
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
        var result = await CreateHandler(Guid.NewGuid()).Handle(
            new RejectMeetingRequestCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WhenMeetingRequestBelongsToAnotherAdvisor_ReturnsForbidden()
    {
        var advisorUserId = Guid.NewGuid();
        SeedCareerAdvisor(advisorUserId);
        var otherAdvisorId = Guid.NewGuid();
        var meetingRequest = GenclikMerkezi.Modules.CareerAdvisor.Domain.MeetingRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(), otherAdvisorId, DateTime.UtcNow);
        _meetingRequestRepository.Add(meetingRequest);

        var result = await CreateHandler(advisorUserId).Handle(
            new RejectMeetingRequestCommand(meetingRequest.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithOwnMeetingRequest_Rejects_AndNotifiesCandidate()
    {
        var advisorUserId = Guid.NewGuid();
        var careerAdvisor = SeedCareerAdvisor(advisorUserId);
        var candidateUserId = Guid.NewGuid();
        var meetingRequest = GenclikMerkezi.Modules.CareerAdvisor.Domain.MeetingRequest.Create(
            Guid.NewGuid(), candidateUserId, careerAdvisor.Id, DateTime.UtcNow);
        _meetingRequestRepository.Add(meetingRequest);
        _identityService.UserProfileResult = new IdentityUserProfile(candidateUserId, "aday@example.com", "Ahmet", "Yılmaz", null);

        var result = await CreateHandler(advisorUserId).Handle(
            new RejectMeetingRequestCommand(meetingRequest.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(GenclikMerkezi.Modules.CareerAdvisor.Domain.MeetingRequestStatus.Reddedildi, meetingRequest.Status);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);

        var notification = Assert.Single(_notificationModuleContract.SentNotifications);
        Assert.Equal(candidateUserId, notification.UserId);
    }

    [Fact]
    public async Task Handle_WhenAlreadyConfirmed_ReturnsConflict()
    {
        var advisorUserId = Guid.NewGuid();
        var careerAdvisor = SeedCareerAdvisor(advisorUserId);
        var meetingRequest = GenclikMerkezi.Modules.CareerAdvisor.Domain.MeetingRequest.Create(
            Guid.NewGuid(), Guid.NewGuid(), careerAdvisor.Id, DateTime.UtcNow);
        meetingRequest.ProposeTime(DateTime.UtcNow.AddDays(1));
        meetingRequest.Confirm(DateTime.UtcNow);
        _meetingRequestRepository.Add(meetingRequest);

        var result = await CreateHandler(advisorUserId).Handle(
            new RejectMeetingRequestCommand(meetingRequest.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }
}
