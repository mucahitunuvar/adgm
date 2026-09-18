using GenclikMerkezi.Modules.Candidate.Features.RequestMeeting;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;

namespace GenclikMerkezi.UnitTests.Candidate.Features.RequestMeeting;

public class RequestMeetingCommandHandlerTests
{
    private readonly FakeCandidateCvRepository _candidateCvRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();

    private RequestMeetingCommandHandler CreateHandler(Guid? currentUserId) =>
        new(_candidateCvRepository, new FakeCurrentUserContext(currentUserId), _careerAdvisorModuleContract);

    [Fact]
    public async Task Handle_WithUnknownCandidateCvId_ReturnsNotFound()
    {
        var result = await CreateHandler(Guid.NewGuid()).Handle(
            new RequestMeetingCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WhenNotOwner_ReturnsForbidden()
    {
        var advisorId = Guid.NewGuid();
        var candidateCv = GenclikMerkezi.Modules.Candidate.Domain.CandidateCv.Create(
            Guid.NewGuid(), "Ahmet", "Yılmaz", "aday@example.com", null, advisorId);
        _candidateCvRepository.Add(candidateCv);

        var result = await CreateHandler(Guid.NewGuid()).Handle(
            new RequestMeetingCommand(candidateCv.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithNoAssignedAdvisor_ReturnsConflict()
    {
        var userId = Guid.NewGuid();
        var candidateCv = GenclikMerkezi.Modules.Candidate.Domain.CandidateCv.Create(
            userId, "Ahmet", "Yılmaz", "aday@example.com", null, null);
        _candidateCvRepository.Add(candidateCv);

        var result = await CreateHandler(userId).Handle(new RequestMeetingCommand(candidateCv.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Null(_careerAdvisorModuleContract.CreateMeetingRequestCall);
    }

    [Fact]
    public async Task Handle_WithAssignedAdvisor_CallsContract_AndReturnsMeetingRequestId()
    {
        var userId = Guid.NewGuid();
        var advisorId = Guid.NewGuid();
        var candidateCv = GenclikMerkezi.Modules.Candidate.Domain.CandidateCv.Create(
            userId, "Ahmet", "Yılmaz", "aday@example.com", null, advisorId);
        _candidateCvRepository.Add(candidateCv);
        var meetingRequestId = Guid.NewGuid();
        _careerAdvisorModuleContract.CreateMeetingRequestResult = Result.Success(meetingRequestId);

        var result = await CreateHandler(userId).Handle(new RequestMeetingCommand(candidateCv.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(meetingRequestId, result.Value.MeetingRequestId);
        Assert.Equal((candidateCv.Id, userId, advisorId), _careerAdvisorModuleContract.CreateMeetingRequestCall);
    }
}
