using GenclikMerkezi.Modules.Candidate.Features.ConfirmMeeting;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;

namespace GenclikMerkezi.UnitTests.Candidate.Features.ConfirmMeeting;

public class ConfirmMeetingCommandHandlerTests
{
    private readonly FakeCandidateCvRepository _candidateCvRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();

    private ConfirmMeetingCommandHandler CreateHandler(Guid? currentUserId) =>
        new(_candidateCvRepository, new FakeCurrentUserContext(currentUserId), _careerAdvisorModuleContract);

    [Fact]
    public async Task Handle_WithUnknownCandidateCvId_ReturnsNotFound()
    {
        var result = await CreateHandler(Guid.NewGuid()).Handle(
            new ConfirmMeetingCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WhenNotOwner_ReturnsForbidden()
    {
        var candidateCv = GenclikMerkezi.Modules.Candidate.Domain.CandidateCv.Create(
            Guid.NewGuid(), "Ahmet", "Yılmaz", "aday@example.com", null, Guid.NewGuid());
        _candidateCvRepository.Add(candidateCv);

        var result = await CreateHandler(Guid.NewGuid()).Handle(
            new ConfirmMeetingCommand(candidateCv.Id, Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Null(_careerAdvisorModuleContract.ConfirmMeetingRequestCall);
    }

    [Fact]
    public async Task Handle_WhenOwner_CallsContractWithOwnUserId()
    {
        var userId = Guid.NewGuid();
        var candidateCv = GenclikMerkezi.Modules.Candidate.Domain.CandidateCv.Create(
            userId, "Ahmet", "Yılmaz", "aday@example.com", null, Guid.NewGuid());
        _candidateCvRepository.Add(candidateCv);
        var meetingRequestId = Guid.NewGuid();
        _careerAdvisorModuleContract.ConfirmMeetingRequestResult = Result.Success();

        var result = await CreateHandler(userId).Handle(
            new ConfirmMeetingCommand(candidateCv.Id, meetingRequestId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal((meetingRequestId, userId), _careerAdvisorModuleContract.ConfirmMeetingRequestCall);
    }

    [Fact]
    public async Task Handle_WhenContractReturnsFailure_PropagatesIt()
    {
        var userId = Guid.NewGuid();
        var candidateCv = GenclikMerkezi.Modules.Candidate.Domain.CandidateCv.Create(
            userId, "Ahmet", "Yılmaz", "aday@example.com", null, Guid.NewGuid());
        _candidateCvRepository.Add(candidateCv);
        var error = Error.Conflict("MeetingRequest.InvalidTransition", "Cannot confirm.");
        _careerAdvisorModuleContract.ConfirmMeetingRequestResult = Result.Failure(error);

        var result = await CreateHandler(userId).Handle(
            new ConfirmMeetingCommand(candidateCv.Id, Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }
}
