using GenclikMerkezi.Modules.Employment.Domain;

namespace GenclikMerkezi.UnitTests.Employment.Domain;

public class EmploymentTests
{
    private static GenclikMerkezi.Modules.Employment.Domain.Employment CreateEmployment(Guid? interviewId = null) =>
        GenclikMerkezi.Modules.Employment.Domain.Employment.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), interviewId, DateTime.UtcNow, Guid.NewGuid(), DateTime.UtcNow);

    [Fact]
    public void Create_SetsAllFieldsAndDefaultsToDevamEdiyor()
    {
        var candidateCvId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var positionId = Guid.NewGuid();
        var interviewId = Guid.NewGuid();
        var startDateUtc = DateTime.UtcNow;
        var createdByAdvisorId = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;

        var employment = GenclikMerkezi.Modules.Employment.Domain.Employment.Create(
            candidateCvId, companyId, positionId, interviewId, startDateUtc, createdByAdvisorId, createdAtUtc);

        Assert.Equal(EmploymentStatus.DevamEdiyor, employment.Status);
        Assert.Equal(candidateCvId, employment.CandidateCvId);
        Assert.Equal(companyId, employment.CompanyId);
        Assert.Equal(positionId, employment.PositionId);
        Assert.Equal(interviewId, employment.InterviewId);
        Assert.Equal(startDateUtc, employment.StartDateUtc);
        Assert.Equal(createdByAdvisorId, employment.CreatedByAdvisorId);
        Assert.Equal(createdAtUtc, employment.CreatedAtUtc);
        Assert.Null(employment.EndDateUtc);
        Assert.Null(employment.DepartureReason);
    }

    [Fact]
    public void Create_WithoutInterviewId_LeavesInterviewIdNull()
    {
        var employment = CreateEmployment(interviewId: null);

        Assert.Null(employment.InterviewId);
    }

    [Fact]
    public void EndEmployment_FromDevamEdiyor_Succeeds()
    {
        var employment = CreateEmployment();
        var endDateUtc = DateTime.UtcNow.AddDays(30);

        var result = employment.EndEmployment("Daha iyi bir fırsat buldu", endDateUtc);

        Assert.True(result.IsSuccess);
        Assert.Equal(EmploymentStatus.SonaErdi, employment.Status);
        Assert.Equal(endDateUtc, employment.EndDateUtc);
        Assert.Equal("Daha iyi bir fırsat buldu", employment.DepartureReason);
    }

    [Fact]
    public void EndEmployment_FromSonaErdi_Fails()
    {
        var employment = CreateEmployment();
        employment.EndEmployment("İlk neden", DateTime.UtcNow);

        var result = employment.EndEmployment("İkinci neden", DateTime.UtcNow.AddDays(1));

        Assert.True(result.IsFailure);
        Assert.Equal("İlk neden", employment.DepartureReason);
    }
}
