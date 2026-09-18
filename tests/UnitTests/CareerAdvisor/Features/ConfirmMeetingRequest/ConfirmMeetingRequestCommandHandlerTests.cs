using GenclikMerkezi.Modules.CareerAdvisor.Features.ConfirmMeetingRequest;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;

namespace GenclikMerkezi.UnitTests.CareerAdvisor.Features.ConfirmMeetingRequest;

public class ConfirmMeetingRequestCommandHandlerTests
{
    private readonly FakeMeetingRequestRepository _meetingRequestRepository = new();
    private readonly FakeCareerAdvisorRepository _careerAdvisorRepository = new();
    private readonly FakeNotificationModuleContract _notificationModuleContract = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private ConfirmMeetingRequestCommandHandler CreateHandler() =>
        new(_meetingRequestRepository, _careerAdvisorRepository, _notificationModuleContract, _unitOfWork);

    private GenclikMerkezi.Modules.CareerAdvisor.Domain.MeetingRequest SeedTarihOnerildiMeetingRequest(
        Guid candidateUserId, out GenclikMerkezi.Modules.CareerAdvisor.Domain.CareerAdvisor careerAdvisor)
    {
        careerAdvisor = GenclikMerkezi.Modules.CareerAdvisor.Domain.CareerAdvisor.Create(
            Guid.NewGuid(), "Ayşe", "Kaya", "danisman@example.com", null, DateTime.UtcNow);
        _careerAdvisorRepository.Add(careerAdvisor);

        var meetingRequest = GenclikMerkezi.Modules.CareerAdvisor.Domain.MeetingRequest.Create(
            Guid.NewGuid(), candidateUserId, careerAdvisor.Id, DateTime.UtcNow);
        meetingRequest.ProposeTime(DateTime.UtcNow.AddDays(1));
        _meetingRequestRepository.Add(meetingRequest);

        return meetingRequest;
    }

    [Fact]
    public async Task Handle_WithUnknownMeetingRequestId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(
            new ConfirmMeetingRequestCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithMismatchedCandidateUserId_ReturnsForbidden()
    {
        var meetingRequest = SeedTarihOnerildiMeetingRequest(Guid.NewGuid(), out _);

        var result = await CreateHandler().Handle(
            new ConfirmMeetingRequestCommand(meetingRequest.Id, Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal(GenclikMerkezi.Modules.CareerAdvisor.Domain.MeetingRequestStatus.TarihOnerildi, meetingRequest.Status);
    }

    [Fact]
    public async Task Handle_WithMatchingCandidateUserId_Confirms_AndNotifiesAdvisor()
    {
        var candidateUserId = Guid.NewGuid();
        var meetingRequest = SeedTarihOnerildiMeetingRequest(
            candidateUserId, out GenclikMerkezi.Modules.CareerAdvisor.Domain.CareerAdvisor careerAdvisor);

        var result = await CreateHandler().Handle(
            new ConfirmMeetingRequestCommand(meetingRequest.Id, candidateUserId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(GenclikMerkezi.Modules.CareerAdvisor.Domain.MeetingRequestStatus.Onaylandi, meetingRequest.Status);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);

        var notification = Assert.Single(_notificationModuleContract.SentNotifications);
        Assert.Equal(careerAdvisor.UserId, notification.UserId);
    }

    [Fact]
    public async Task Handle_WhenAlreadyRejected_ReturnsConflict()
    {
        var candidateUserId = Guid.NewGuid();
        var meetingRequest = SeedTarihOnerildiMeetingRequest(candidateUserId, out _);
        meetingRequest.Reject();

        var result = await CreateHandler().Handle(
            new ConfirmMeetingRequestCommand(meetingRequest.Id, candidateUserId), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }
}
