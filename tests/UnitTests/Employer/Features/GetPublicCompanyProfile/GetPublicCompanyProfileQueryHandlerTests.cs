using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.GetPublicCompanyProfile;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;
using GenclikMerkezi.UnitTests.ReferenceData.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.GetPublicCompanyProfile;

public class GetPublicCompanyProfileQueryHandlerTests
{
    private readonly FakeCompanyRepository _companyRepository = new();
    private readonly FakeJobRepository _jobRepository = new();
    private readonly FakeReferenceDataLookupReader _referenceDataLookupReader = new();
    private readonly FakeHtmlContentSanitizer _htmlContentSanitizer = new();

    private GetPublicCompanyProfileQueryHandler CreateHandler() =>
        new(_companyRepository, _jobRepository, _referenceDataLookupReader, _htmlContentSanitizer);

    private static Company CreateApprovedCompany(Guid sectorId, Guid provinceId)
    {
        var company = Company.Create(
            Guid.NewGuid(), "Acme A.Ş.", sectorId, 2010, 50, "https://acme.example.com", Guid.NewGuid(), provinceId,
            Guid.NewGuid(), "Adres", "<script>alert(1)</script><p>Hakkımızda</p>", "Ayşe", "Kaya",
            "firma@example.com", "05550000000", Guid.NewGuid(), "1234567890", false, null, DateTime.UtcNow);
        company.Approve(Guid.NewGuid(), DateTime.UtcNow);
        return company;
    }

    [Theory]
    [InlineData(CompanyStatus.PendingApproval)]
    [InlineData(CompanyStatus.Rejected)]
    [InlineData(CompanyStatus.Deactivated)]
    public async Task Handle_WithNonApprovedCompany_ReturnsNotFound(CompanyStatus status)
    {
        var company = Company.Create(
            Guid.NewGuid(), "Acme A.Ş.", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "Adres", null, "Ayşe", "Kaya", "firma@example.com", "05550000000", Guid.NewGuid(),
            "1234567890", false, null, DateTime.UtcNow);
        if (status == CompanyStatus.Rejected)
        {
            company.Reject("Eksik belge", DateTime.UtcNow);
        }
        else if (status == CompanyStatus.Deactivated)
        {
            company.Approve(Guid.NewGuid(), DateTime.UtcNow);
            company.Deactivate(Guid.NewGuid(), DateTime.UtcNow);
        }

        _companyRepository.Add(company);

        var result = await CreateHandler().Handle(new GetPublicCompanyProfileQuery(company.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new GetPublicCompanyProfileQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithApprovedCompany_ReturnsSanitizedProfileWithResolvedNames()
    {
        var sectorId = Guid.NewGuid();
        var provinceId = Guid.NewGuid();
        var company = CreateApprovedCompany(sectorId, provinceId);
        _companyRepository.Add(company);
        _referenceDataLookupReader.SeedList(
            ReferenceDataLookupType.Sector, new LookupItemSummary(sectorId, "TEK", "Teknoloji", true, 0));
        _referenceDataLookupReader.SeedList(
            ReferenceDataLookupType.Province, new LookupItemSummary(provinceId, "34", "İstanbul", true, 0));

        var result = await CreateHandler().Handle(new GetPublicCompanyProfileQuery(company.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(company.Id, result.Value.Id);
        Assert.Equal("Acme A.Ş.", result.Value.Name);
        Assert.Equal("Teknoloji", result.Value.SectorName);
        Assert.Equal("İstanbul", result.Value.ProvinceName);
        Assert.Equal(2010, result.Value.FoundedYear);
        Assert.Equal(50, result.Value.EmployeeCount);
        Assert.False(result.Value.HasLogo);
        Assert.Equal(0, result.Value.PublishedJobCount);
        Assert.Contains(company.AboutHtml!, _htmlContentSanitizer.SanitizeCalledWith);
    }

    [Fact]
    public async Task Handle_HasLogo_IsTrue_OnlyWhenLogoExistsAndShowLogoOnWebsiteIsEnabled()
    {
        var company = CreateApprovedCompany(Guid.NewGuid(), Guid.NewGuid());
        company.SetLogo(FileAttachment.Create(
            "employer-logos/2026/09/21/logo.png", "logo.png", "image/png", 1024, DateTime.UtcNow, "Company", company.Id));
        _companyRepository.Add(company);

        var beforeConsentResult = await CreateHandler().Handle(new GetPublicCompanyProfileQuery(company.Id), CancellationToken.None);
        Assert.False(beforeConsentResult.Value.HasLogo);

        company.SetShowLogoOnWebsite(true);
        var afterConsentResult = await CreateHandler().Handle(new GetPublicCompanyProfileQuery(company.Id), CancellationToken.None);
        Assert.True(afterConsentResult.Value.HasLogo);
    }
}
