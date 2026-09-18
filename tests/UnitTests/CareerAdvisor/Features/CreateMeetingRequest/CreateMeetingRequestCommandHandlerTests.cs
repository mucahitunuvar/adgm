using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateMeetingRequest;
using GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;

namespace GenclikMerkezi.UnitTests.CareerAdvisor.Features.CreateMeetingRequest;

public class CreateMeetingRequestCommandHandlerTests
{
    private readonly FakeCareerAdvisorRepository _careerAdvisorRepository = new();
    private readonly FakeMeetingRequestRepository _meetingRequestRepository = new();
    private readonly FakeNotificationModuleContract _notificationModuleContract = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CreateMeetingRequestCommandHandler CreateHandler() =>
        new(_careerAdvisorRepository, _meetingRequestRepository, _notificationModuleContract, _unitOfWork);

    [Fact]
    public async Task Handle_WithUnknownCareerAdvisorId_ReturnsNotFound()
    {
        var command = new CreateMeetingRequestCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Empty(_meetingRequestRepository.MeetingRequests);
    }

    [Fact]
    public async Task Handle_WithKnownCareerAdvisor_CreatesMeetingRequest_AndNotifiesAdvisor()
    {
        var careerAdvisor = GenclikMerkezi.Modules.CareerAdvisor.Domain.CareerAdvisor.Create(
            Guid.NewGuid(), "Ayşe", "Kaya", "danisman@example.com", null, DateTime.UtcNow);
        _careerAdvisorRepository.Add(careerAdvisor);
        var candidateCvId = Guid.NewGuid();
        var candidateUserId = Guid.NewGuid();
        var command = new CreateMeetingRequestCommand(candidateCvId, candidateUserId, careerAdvisor.Id);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var meetingRequest = Assert.Single(_meetingRequestRepository.MeetingRequests);
        Assert.Equal(candidateCvId, meetingRequest.CandidateCvId);
        Assert.Equal(candidateUserId, meetingRequest.CandidateUserId);
        Assert.Equal(careerAdvisor.Id, meetingRequest.CareerAdvisorId);
        Assert.Equal(result.Value, meetingRequest.Id);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);

        var notification = Assert.Single(_notificationModuleContract.SentNotifications);
        Assert.Equal(careerAdvisor.UserId, notification.UserId);
        Assert.Equal(careerAdvisor.Email, notification.RecipientEmail);
    }
}
