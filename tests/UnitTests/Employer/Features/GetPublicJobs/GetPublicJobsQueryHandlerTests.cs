using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.GetPublicJobs;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;
using GenclikMerkezi.UnitTests.ReferenceData.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.GetPublicJobs;

public class GetPublicJobsQueryHandlerTests
{
    private readonly FakeJobRepository _jobRepository = new();
    private readonly FakeReferenceDataLookupReader _referenceDataLookupReader = new();

    private GetPublicJobsQueryHandler CreateHandler() => new(_jobRepository, _referenceDataLookupReader);

    private static GetPublicJobsQuery EmptyQuery(int page = 1, int pageSize = 20, string? q = null) =>
        new(null, null, null, null, null, null, null, q) { Page = page, PageSize = pageSize };

    private static Job CreatePublishedJob(
        string title = "Kaynakçı",
        Guid? provinceId = null,
        Guid? employmentTypeId = null,
        Guid? workLocationTypeId = null,
        Guid? positionId = null,
        Guid? departmentId = null,
        DateTime? publishedAtUtc = null)
    {
        var job = Job.Create(
            Guid.NewGuid(), title, false, employmentTypeId ?? Guid.NewGuid(), workLocationTypeId ?? Guid.NewGuid(),
            positionId ?? Guid.NewGuid(), departmentId ?? Guid.NewGuid(), provinceId ?? Guid.NewGuid(),
            "<p>Açıklama</p>", Guid.NewGuid(), [Guid.NewGuid()], [], [], [], [], DateTime.UtcNow);
        job.Submit();
        job.Approve(Guid.NewGuid(), publishedAtUtc ?? DateTime.UtcNow);
        return job;
    }

    private void SeedPublishedJobWithApprovedCompany(Job job, string companyName = "Acme A.Ş.", bool companyHasLogo = false)
    {
        _jobRepository.Add(job);
        _jobRepository.RegisterCompany(job.CompanyId, companyName, companyHasLogo, CompanyStatus.Approved);
    }

    [Fact]
    public async Task Handle_WithPublishedJobAndApprovedCompany_ReturnsItem()
    {
        var job = CreatePublishedJob();
        SeedPublishedJobWithApprovedCompany(job, "Acme A.Ş.", companyHasLogo: true);

        var result = await CreateHandler().Handle(EmptyQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal(job.Id, item.Id);
        Assert.Equal(job.Slug, item.Slug);
        Assert.Equal("Acme A.Ş.", item.Company.Name);
        Assert.True(item.Company.HasLogo);
        Assert.Equal("Açıklama", item.Summary);
    }

    [Theory]
    [InlineData(JobStatus.Draft, CompanyStatus.Approved, false)]
    [InlineData(JobStatus.UnderReview, CompanyStatus.Approved, false)]
    [InlineData(JobStatus.RevisionRequested, CompanyStatus.Approved, false)]
    [InlineData(JobStatus.Rejected, CompanyStatus.Approved, false)]
    [InlineData(JobStatus.SuspendedByAdmin, CompanyStatus.Approved, false)]
    [InlineData(JobStatus.Published, CompanyStatus.PendingApproval, false)]
    [InlineData(JobStatus.Published, CompanyStatus.Rejected, false)]
    [InlineData(JobStatus.Published, CompanyStatus.Deactivated, false)]
    [InlineData(JobStatus.Published, CompanyStatus.Approved, true)]
    public async Task Handle_OnlyReturnsPublishedJobsWithApprovedCompany(
        JobStatus jobStatus, CompanyStatus companyStatus, bool shouldAppear)
    {
        var job = Job.Create(
            Guid.NewGuid(), "Kaynakçı", false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "<p>Açıklama</p>", Guid.NewGuid(), [], [], [], [], [], DateTime.UtcNow);
        TransitionJobTo(job, jobStatus);
        _jobRepository.Add(job);
        _jobRepository.RegisterCompany(job.CompanyId, "Acme A.Ş.", false, companyStatus);

        var result = await CreateHandler().Handle(EmptyQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(shouldAppear ? 1 : 0, result.Value.Items.Count);
    }

    private static void TransitionJobTo(Job job, JobStatus status)
    {
        if (status == JobStatus.Draft)
        {
            return;
        }

        job.Submit();

        if (status == JobStatus.UnderReview)
        {
            return;
        }

        if (status == JobStatus.Rejected)
        {
            job.Reject("Eksik bilgi", DateTime.UtcNow);
            return;
        }

        if (status == JobStatus.RevisionRequested)
        {
            job.RequestRevision("Lütfen detaylandırın", DateTime.UtcNow);
            return;
        }

        job.Approve(Guid.NewGuid(), DateTime.UtcNow);

        if (status == JobStatus.SuspendedByAdmin)
        {
            job.Suspend(Guid.NewGuid(), "Uygunsuz içerik", DateTime.UtcNow);
        }
    }

    [Fact]
    public async Task Handle_FiltersByProvinceId()
    {
        var matchingProvinceId = Guid.NewGuid();
        var matchingJob = CreatePublishedJob(provinceId: matchingProvinceId);
        var otherJob = CreatePublishedJob(provinceId: Guid.NewGuid());
        SeedPublishedJobWithApprovedCompany(matchingJob);
        SeedPublishedJobWithApprovedCompany(otherJob);

        var query = EmptyQuery() with { ProvinceId = matchingProvinceId };
        var result = await CreateHandler().Handle(query, CancellationToken.None);

        var item = Assert.Single(result.Value.Items);
        Assert.Equal(matchingJob.Id, item.Id);
    }

    [Fact]
    public async Task Handle_SearchMatchesJobTitleOrCompanyName()
    {
        var jobTitleMatch = CreatePublishedJob(title: "Kaynakçı Aranıyor");
        var companyNameMatch = CreatePublishedJob(title: "Depo Görevlisi");
        var noMatch = CreatePublishedJob(title: "Satış Danışmanı");
        SeedPublishedJobWithApprovedCompany(jobTitleMatch, "Acme A.Ş.");
        SeedPublishedJobWithApprovedCompany(companyNameMatch, "Kaynak İnsan Kaynakları A.Ş.");
        SeedPublishedJobWithApprovedCompany(noMatch, "Beta Ltd.");

        var result = await CreateHandler().Handle(EmptyQuery(q: "kaynak"), CancellationToken.None);

        Assert.Equal(2, result.Value.Items.Count);
        Assert.Contains(result.Value.Items, i => i.Id == jobTitleMatch.Id);
        Assert.Contains(result.Value.Items, i => i.Id == companyNameMatch.Id);
    }

    [Fact]
    public async Task Handle_WithSearchShorterThanMinimum_ReturnsValidationError()
    {
        var result = await CreateHandler().Handle(EmptyQuery(q: "a"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task Handle_WithSearchLongerThanMaximum_ReturnsValidationError()
    {
        var result = await CreateHandler().Handle(EmptyQuery(q: new string('a', 101)), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public async Task Handle_OrdersByPublishedAtDescending()
    {
        var older = CreatePublishedJob(title: "Eski İlan", publishedAtUtc: DateTime.UtcNow.AddDays(-2));
        var newer = CreatePublishedJob(title: "Yeni İlan", publishedAtUtc: DateTime.UtcNow);
        SeedPublishedJobWithApprovedCompany(older);
        SeedPublishedJobWithApprovedCompany(newer);

        var result = await CreateHandler().Handle(EmptyQuery(), CancellationToken.None);

        Assert.Equal(newer.Id, result.Value.Items[0].Id);
        Assert.Equal(older.Id, result.Value.Items[1].Id);
    }

    [Fact]
    public async Task Handle_ResolvesLookupNames()
    {
        var provinceId = Guid.NewGuid();
        var job = CreatePublishedJob(provinceId: provinceId);
        SeedPublishedJobWithApprovedCompany(job);
        _referenceDataLookupReader.SeedList(ReferenceDataLookupType.Province, new LookupItemSummary(provinceId, "34", "İstanbul", true, 0));

        var result = await CreateHandler().Handle(EmptyQuery(), CancellationToken.None);

        Assert.Equal("İstanbul", result.Value.Items.Single().ProvinceName);
    }
}
