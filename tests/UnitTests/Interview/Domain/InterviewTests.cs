using GenclikMerkezi.Modules.Interview.Domain;

namespace GenclikMerkezi.UnitTests.Interview.Domain;

public class InterviewTests
{
    private static GenclikMerkezi.Modules.Interview.Domain.Interview CreateInterview(
        InterviewRequestedByRole requestedByRole = InterviewRequestedByRole.Candidate) =>
        GenclikMerkezi.Modules.Interview.Domain.Interview.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), requestedByRole, DateTime.UtcNow);

    [Fact]
    public void Create_SetsAllFieldsAndDefaultsToTalepEdildi()
    {
        var candidateCvId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var organizingAdvisorId = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;

        var interview = GenclikMerkezi.Modules.Interview.Domain.Interview.Create(
            candidateCvId, companyId, organizingAdvisorId, InterviewRequestedByRole.Employer, createdAtUtc);

        Assert.Equal(InterviewStatus.TalepEdildi, interview.Status);
        Assert.Equal(candidateCvId, interview.CandidateCvId);
        Assert.Equal(companyId, interview.CompanyId);
        Assert.Equal(organizingAdvisorId, interview.OrganizingAdvisorId);
        Assert.Equal(InterviewRequestedByRole.Employer, interview.RequestedByRole);
        Assert.Equal(createdAtUtc, interview.CreatedAtUtc);
        Assert.Null(interview.ScheduledAtUtc);
        Assert.Null(interview.Outcome);
        Assert.Null(interview.ResultNotes);
    }

    [Fact]
    public void Schedule_FromTalepEdildi_Succeeds()
    {
        var interview = CreateInterview();
        var scheduledAtUtc = DateTime.UtcNow.AddDays(3);

        var result = interview.Schedule(scheduledAtUtc);

        Assert.True(result.IsSuccess);
        Assert.Equal(InterviewStatus.Planlandi, interview.Status);
        Assert.Equal(scheduledAtUtc, interview.ScheduledAtUtc);
    }

    [Fact]
    public void Schedule_FromNonTalepEdildiStatus_Fails()
    {
        var interview = CreateInterview();
        interview.Schedule(DateTime.UtcNow.AddDays(3));

        var result = interview.Schedule(DateTime.UtcNow.AddDays(5));

        Assert.True(result.IsFailure);
        Assert.Equal(InterviewStatus.Planlandi, interview.Status);
    }

    [Fact]
    public void RecordResult_FromPlanlandi_Succeeds()
    {
        var interview = CreateInterview();
        interview.Schedule(DateTime.UtcNow.AddDays(3));

        var result = interview.RecordResult(InterviewResult.Olumlu, "İyi geçti", DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(InterviewStatus.Tamamlandi, interview.Status);
        Assert.Equal(InterviewResult.Olumlu, interview.Outcome);
        Assert.Equal("İyi geçti", interview.ResultNotes);
    }

    [Fact]
    public void RecordResult_FromTalepEdildi_Fails()
    {
        var interview = CreateInterview();

        var result = interview.RecordResult(InterviewResult.Olumlu, null, DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(InterviewStatus.TalepEdildi, interview.Status);
    }

    [Theory]
    [InlineData(InterviewStatus.TalepEdildi)]
    [InlineData(InterviewStatus.Planlandi)]
    public void Cancel_FromTalepEdildiOrPlanlandi_Succeeds(InterviewStatus initialStatus)
    {
        var interview = CreateInterview();

        if (initialStatus == InterviewStatus.Planlandi)
        {
            interview.Schedule(DateTime.UtcNow.AddDays(3));
        }

        var result = interview.Cancel();

        Assert.True(result.IsSuccess);
        Assert.Equal(InterviewStatus.IptalEdildi, interview.Status);
    }

    [Fact]
    public void Cancel_FromTamamlandi_Fails()
    {
        var interview = CreateInterview();
        interview.Schedule(DateTime.UtcNow.AddDays(3));
        interview.RecordResult(InterviewResult.Olumlu, null, DateTime.UtcNow);

        var result = interview.Cancel();

        Assert.True(result.IsFailure);
        Assert.Equal(InterviewStatus.Tamamlandi, interview.Status);
    }
}
