using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.GetPublicJobDetail;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;
using GenclikMerkezi.UnitTests.ReferenceData.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.GetPublicJobDetail;

public class GetPublicJobDetailQueryHandlerTests
{
    private readonly FakeJobRepository _jobRepository = new();
    private readonly FakeCompanyRepository _companyRepository = new();
    private readonly FakeReferenceDataLookupReader _referenceDataLookupReader = new();
    private readonly FakeHtmlContentSanitizer _htmlContentSanitizer = new();

    private GetPublicJobDetailQueryHandler CreateHandler() =>
        new(_jobRepository, _companyRepository, _referenceDataLookupReader, _htmlContentSanitizer);

    private static Company CreateApprovedCompany(Guid? id = null)
    {
        var company = Company.Create(
            Guid.NewGuid(), "Acme A.Ş.", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "Adres", null, "Ayşe", "Kaya", "firma@example.com", "05550000000", Guid.NewGuid(),
            "1234567890", false, null, DateTime.UtcNow);
        company.Approve(Guid.NewGuid(), DateTime.UtcNow);
        return company;
    }

    private static Job CreatePublishedJob(
        Guid companyId, Guid militaryStatusId, Guid educationLevelId, Guid drivingLicenseId,
        Guid languageId, Guid languageLevelId)
    {
        var job = Job.Create(
            companyId, "Kaynakçı", true, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "<p>Açıklama</p><script>alert(1)</script>", Guid.NewGuid(),
            [Guid.NewGuid()], [militaryStatusId], [educationLevelId], [drivingLicenseId],
            [(languageId, languageLevelId)], DateTime.UtcNow);
        job.Submit();
        job.Approve(Guid.NewGuid(), DateTime.UtcNow);
        return job;
    }

    [Fact]
    public async Task Handle_WithPublishedJobAndApprovedCompany_ReturnsFullyResolvedDetail()
    {
        var company = CreateApprovedCompany();
        var militaryStatusId = Guid.NewGuid();
        var educationLevelId = Guid.NewGuid();
        var drivingLicenseId = Guid.NewGuid();
        var languageId = Guid.NewGuid();
        var languageLevelId = Guid.NewGuid();
        var job = CreatePublishedJob(company.Id, militaryStatusId, educationLevelId, drivingLicenseId, languageId, languageLevelId);
        _companyRepository.Add(company);
        _jobRepository.Add(job);

        _referenceDataLookupReader.SeedList(
            ReferenceDataLookupType.MilitaryStatus, new LookupItemSummary(militaryStatusId, "M1", "Muaf", true, 0));
        _referenceDataLookupReader.SeedList(
            ReferenceDataLookupType.EducationLevel, new LookupItemSummary(educationLevelId, "E1", "Lisans", true, 0));
        _referenceDataLookupReader.SeedList(
            ReferenceDataLookupType.DriversLicenseType, new LookupItemSummary(drivingLicenseId, "B", "B Sınıfı", true, 0));
        _referenceDataLookupReader.SeedList(
            ReferenceDataLookupType.Language, new LookupItemSummary(languageId, "EN", "İngilizce", true, 0));
        _referenceDataLookupReader.SeedList(
            ReferenceDataLookupType.LanguageLevel, new LookupItemSummary(languageLevelId, "B2", "Orta Üstü", true, 0));

        var result = await CreateHandler().Handle(new GetPublicJobDetailQuery(job.Slug!), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var response = result.Value;
        Assert.Equal(job.Id, response.Id);
        Assert.Equal(job.Slug, response.Slug);
        Assert.Equal(company.Name, response.Company.Name);
        Assert.Equal("Muaf", Assert.Single(response.MilitaryStatusPreferenceNames));
        Assert.Equal("Lisans", Assert.Single(response.EducationLevelNames));
        Assert.Equal("B Sınıfı", Assert.Single(response.DrivingLicenseNames));
        var languageRequirement = Assert.Single(response.LanguageRequirements);
        Assert.Equal("İngilizce", languageRequirement.LanguageName);
        Assert.Equal("Orta Üstü", languageRequirement.LevelName);

        // DescriptionHtml sanitize edilmiş olarak döndürülmeli (FakeHtmlContentSanitizer çağrıldığını kaydeder).
        Assert.Contains(job.DescriptionHtml!, _htmlContentSanitizer.SanitizeCalledWith);
    }

    [Fact]
    public async Task Handle_WithUnknownSlug_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new GetPublicJobDetailQuery("bilinmeyen-slug-12345678"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithJobNotPublished_ReturnsNotFound()
    {
        var company = CreateApprovedCompany();
        var job = Job.Create(
            company.Id, "Kaynakçı", false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "<p>Açıklama</p>", Guid.NewGuid(), [], [], [], [], [], DateTime.UtcNow);
        job.Submit();
        job.Approve(Guid.NewGuid(), DateTime.UtcNow);
        job.Suspend(Guid.NewGuid(), "Uygunsuz içerik", DateTime.UtcNow);
        _companyRepository.Add(company);
        _jobRepository.Add(job);

        var result = await CreateHandler().Handle(new GetPublicJobDetailQuery(job.Slug!), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Theory]
    [InlineData(CompanyStatus.PendingApproval)]
    [InlineData(CompanyStatus.Rejected)]
    [InlineData(CompanyStatus.Deactivated)]
    public async Task Handle_WithCompanyNotApproved_ReturnsNotFound(CompanyStatus companyStatus)
    {
        var company = Company.Create(
            Guid.NewGuid(), "Acme A.Ş.", Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "Adres", null, "Ayşe", "Kaya", "firma@example.com", "05550000000", Guid.NewGuid(),
            "1234567890", false, null, DateTime.UtcNow);
        if (companyStatus == CompanyStatus.Rejected)
        {
            company.Reject("Eksik belge", DateTime.UtcNow);
        }
        else if (companyStatus == CompanyStatus.Deactivated)
        {
            company.Approve(Guid.NewGuid(), DateTime.UtcNow);
            company.Deactivate(Guid.NewGuid(), DateTime.UtcNow);
        }

        var job = Job.Create(
            company.Id, "Kaynakçı", false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "<p>Açıklama</p>", Guid.NewGuid(), [], [], [], [], [], DateTime.UtcNow);
        job.Submit();
        job.Approve(Guid.NewGuid(), DateTime.UtcNow);
        _companyRepository.Add(company);
        _jobRepository.Add(job);

        var result = await CreateHandler().Handle(new GetPublicJobDetailQuery(job.Slug!), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
