using GenclikMerkezi.Modules.Employer.Domain;

namespace GenclikMerkezi.UnitTests.Employer.Domain;

public class CompanyTests
{
    private static Company CreateCompany(Guid? careerAdvisorId = null) =>
        Company.Create(
            userId: Guid.NewGuid(),
            name: "Acme A.Ş.",
            sectorId: Guid.NewGuid(),
            foundedYear: 2010,
            employeeCount: 50,
            websiteUrl: "https://acme.example.com",
            countryId: Guid.NewGuid(),
            provinceId: Guid.NewGuid(),
            districtId: Guid.NewGuid(),
            address: "Örnek Mah. Örnek Cad. No:1",
            aboutHtml: "<p>Hakkımızda</p>",
            contactFirstName: "Ayşe",
            contactLastName: "Kaya",
            contactEmail: "ayse@acme.example.com",
            contactPhone: "05551234567",
            taxOfficeId: Guid.NewGuid(),
            taxNumber: "1234567890",
            marketingConsent: true,
            careerAdvisorId: careerAdvisorId,
            createdAtUtc: DateTime.UtcNow);

    [Fact]
    public void Create_SetsAllFieldsAndDefaultsToPendingApproval()
    {
        var careerAdvisorId = Guid.NewGuid();

        var company = CreateCompany(careerAdvisorId);

        Assert.Equal(CompanyStatus.PendingApproval, company.Status);
        Assert.Equal(careerAdvisorId, company.CareerAdvisorId);
        Assert.Equal("Acme A.Ş.", company.Name);
        Assert.Equal("1234567890", company.TaxNumber);
        Assert.Null(company.ApprovedByUserId);
        Assert.Null(company.ApprovedAtUtc);
        Assert.Null(company.RejectionReason);
        Assert.Null(company.DeactivatedByUserId);
        Assert.Null(company.DeactivatedAtUtc);
    }

    [Fact]
    public void Approve_FromPendingApproval_Succeeds()
    {
        var company = CreateCompany();
        var approvedByUserId = Guid.NewGuid();
        var approvedAtUtc = DateTime.UtcNow;

        var result = company.Approve(approvedByUserId, approvedAtUtc);

        Assert.True(result.IsSuccess);
        Assert.Equal(CompanyStatus.Approved, company.Status);
        Assert.Equal(approvedByUserId, company.ApprovedByUserId);
        Assert.Equal(approvedAtUtc, company.ApprovedAtUtc);
    }

    [Theory]
    [InlineData(CompanyStatus.Approved)]
    [InlineData(CompanyStatus.Rejected)]
    [InlineData(CompanyStatus.Deactivated)]
    public void Approve_FromNonPendingApprovalStatus_Fails(CompanyStatus initialStatus)
    {
        var company = TransitionTo(initialStatus);

        var result = company.Approve(Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(initialStatus, company.Status);
    }

    [Fact]
    public void Reject_FromPendingApproval_Succeeds()
    {
        var company = CreateCompany();

        var result = company.Reject("Eksik belge", DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(CompanyStatus.Rejected, company.Status);
        Assert.Equal("Eksik belge", company.RejectionReason);
    }

    [Theory]
    [InlineData(CompanyStatus.Approved)]
    [InlineData(CompanyStatus.Rejected)]
    [InlineData(CompanyStatus.Deactivated)]
    public void Reject_FromNonPendingApprovalStatus_Fails(CompanyStatus initialStatus)
    {
        var company = TransitionTo(initialStatus);

        var result = company.Reject("Eksik belge", DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(initialStatus, company.Status);
    }

    [Fact]
    public void Deactivate_FromApproved_Succeeds()
    {
        var company = TransitionTo(CompanyStatus.Approved);
        var deactivatedByUserId = Guid.NewGuid();
        var deactivatedAtUtc = DateTime.UtcNow;

        var result = company.Deactivate(deactivatedByUserId, deactivatedAtUtc);

        Assert.True(result.IsSuccess);
        Assert.Equal(CompanyStatus.Deactivated, company.Status);
        Assert.Equal(deactivatedByUserId, company.DeactivatedByUserId);
        Assert.Equal(deactivatedAtUtc, company.DeactivatedAtUtc);
    }

    [Theory]
    [InlineData(CompanyStatus.PendingApproval)]
    [InlineData(CompanyStatus.Rejected)]
    [InlineData(CompanyStatus.Deactivated)]
    public void Deactivate_FromNonApprovedStatus_Fails(CompanyStatus initialStatus)
    {
        var company = TransitionTo(initialStatus);

        var result = company.Deactivate(Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(initialStatus, company.Status);
    }

    [Fact]
    public void AssignCareerAdvisor_UpdatesCareerAdvisorId()
    {
        var company = CreateCompany();
        var newCareerAdvisorId = Guid.NewGuid();

        company.AssignCareerAdvisor(newCareerAdvisorId);

        Assert.Equal(newCareerAdvisorId, company.CareerAdvisorId);
    }

    private static Company TransitionTo(CompanyStatus status)
    {
        var company = CreateCompany();

        if (status == CompanyStatus.PendingApproval)
        {
            return company;
        }

        if (status == CompanyStatus.Approved)
        {
            company.Approve(Guid.NewGuid(), DateTime.UtcNow);
            return company;
        }

        if (status == CompanyStatus.Rejected)
        {
            company.Reject("Eksik belge", DateTime.UtcNow);
            return company;
        }

        company.Approve(Guid.NewGuid(), DateTime.UtcNow);
        company.Deactivate(Guid.NewGuid(), DateTime.UtcNow);
        return company;
    }
}
