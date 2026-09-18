using GenclikMerkezi.Modules.CareerAdvisor.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.CareerAdvisor.Domain;

public class MeetingRequestTests
{
    private static MeetingRequest CreateMeetingRequest() =>
        MeetingRequest.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

    [Fact]
    public void Create_StartsInTalepEdildiStatus()
    {
        var meetingRequest = CreateMeetingRequest();

        Assert.Equal(MeetingRequestStatus.TalepEdildi, meetingRequest.Status);
        Assert.Null(meetingRequest.ProposedDateTimeUtc);
        Assert.Null(meetingRequest.ConfirmedAtUtc);
    }

    [Fact]
    public void ProposeTime_FromTalepEdildi_Succeeds_AndTransitionsToTarihOnerildi()
    {
        var meetingRequest = CreateMeetingRequest();
        var proposedDateTimeUtc = DateTime.UtcNow.AddDays(3);

        var result = meetingRequest.ProposeTime(proposedDateTimeUtc);

        Assert.True(result.IsSuccess);
        Assert.Equal(MeetingRequestStatus.TarihOnerildi, meetingRequest.Status);
        Assert.Equal(proposedDateTimeUtc, meetingRequest.ProposedDateTimeUtc);
    }

    [Theory]
    [InlineData(MeetingRequestStatus.TarihOnerildi)]
    [InlineData(MeetingRequestStatus.Onaylandi)]
    [InlineData(MeetingRequestStatus.Reddedildi)]
    public void ProposeTime_FromNonTalepEdildiStatus_Fails(MeetingRequestStatus initialStatus)
    {
        var meetingRequest = TransitionTo(initialStatus);

        var result = meetingRequest.ProposeTime(DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public void Confirm_FromTarihOnerildi_Succeeds_AndTransitionsToOnaylandi()
    {
        var meetingRequest = CreateMeetingRequest();
        meetingRequest.ProposeTime(DateTime.UtcNow.AddDays(1));
        var confirmedAtUtc = DateTime.UtcNow;

        var result = meetingRequest.Confirm(confirmedAtUtc);

        Assert.True(result.IsSuccess);
        Assert.Equal(MeetingRequestStatus.Onaylandi, meetingRequest.Status);
        Assert.Equal(confirmedAtUtc, meetingRequest.ConfirmedAtUtc);
    }

    [Fact]
    public void Confirm_FromReddedildi_Fails()
    {
        var meetingRequest = TransitionTo(MeetingRequestStatus.Reddedildi);

        var result = meetingRequest.Confirm(DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(MeetingRequestStatus.Reddedildi, meetingRequest.Status);
    }

    [Theory]
    [InlineData(MeetingRequestStatus.TalepEdildi)]
    [InlineData(MeetingRequestStatus.Onaylandi)]
    public void Confirm_FromNonTarihOnerildiStatus_Fails(MeetingRequestStatus initialStatus)
    {
        var meetingRequest = TransitionTo(initialStatus);

        var result = meetingRequest.Confirm(DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Theory]
    [InlineData(MeetingRequestStatus.TalepEdildi)]
    [InlineData(MeetingRequestStatus.TarihOnerildi)]
    public void Reject_FromTalepEdildiOrTarihOnerildi_Succeeds(MeetingRequestStatus initialStatus)
    {
        var meetingRequest = TransitionTo(initialStatus);

        var result = meetingRequest.Reject();

        Assert.True(result.IsSuccess);
        Assert.Equal(MeetingRequestStatus.Reddedildi, meetingRequest.Status);
    }

    [Theory]
    [InlineData(MeetingRequestStatus.Onaylandi)]
    [InlineData(MeetingRequestStatus.Reddedildi)]
    public void Reject_FromOnaylandiOrReddedildi_Fails(MeetingRequestStatus initialStatus)
    {
        var meetingRequest = TransitionTo(initialStatus);

        var result = meetingRequest.Reject();

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    private static MeetingRequest TransitionTo(MeetingRequestStatus status)
    {
        var meetingRequest = CreateMeetingRequest();

        if (status == MeetingRequestStatus.TalepEdildi)
        {
            return meetingRequest;
        }

        meetingRequest.ProposeTime(DateTime.UtcNow.AddDays(1));

        if (status == MeetingRequestStatus.TarihOnerildi)
        {
            return meetingRequest;
        }

        if (status == MeetingRequestStatus.Onaylandi)
        {
            meetingRequest.Confirm(DateTime.UtcNow);
            return meetingRequest;
        }

        meetingRequest.Reject();
        return meetingRequest;
    }
}
