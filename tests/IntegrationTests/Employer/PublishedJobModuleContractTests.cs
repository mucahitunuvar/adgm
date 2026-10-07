using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Employer;
using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Employer;

// Görev 4 (Employer public jobs master prompt): IPublishedJobModuleContract'ın gerçek LocalDB'ye
// karşı doğrulanması - PersonnelNeedModuleContractTests'teki aynı desen (henüz hiçbir tüketici
// olmadığı için doğrudan DI'dan çözülüp çağrılıyor; Website'in kendi arama adaptörü Faz 5'te ayrıca
// yazılacak). HTTP akışı kullanılmıyor - 250 ilan için register/submit/approve round trip'leri bu
// testi anlamsız şekilde yavaşlatırdı; repository'ler doğrudan DI'dan çözülüp domain metodları
// çağrılıyor.
public class PublishedJobModuleContractTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public PublishedJobModuleContractTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static Company CreateApprovedCompany(string name)
    {
        var company = Company.Create(
            Guid.NewGuid(), name, Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Adres", null, "Ad", "Soyad", $"firma{Guid.NewGuid():N}@example.com", "05550000000", Guid.NewGuid(),
            Random.Shared.NextInt64(1_000_000_000L, 9_999_999_999L).ToString(), false, null, DateTime.UtcNow);
        company.Approve(Guid.NewGuid(), DateTime.UtcNow);
        return company;
    }

    private static Company CreatePendingCompany(string name) =>
        Company.Create(
            Guid.NewGuid(), name, Guid.NewGuid(), null, null, null, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Adres", null, "Ad", "Soyad", $"firma{Guid.NewGuid():N}@example.com", "05550000000", Guid.NewGuid(),
            Random.Shared.NextInt64(1_000_000_000L, 9_999_999_999L).ToString(), false, null, DateTime.UtcNow);

    private static Job CreatePublishedJob(Guid companyId, string title, DateTime createdAtUtc, DateTime publishedAtUtc)
    {
        var job = Job.Create(
            companyId, title, false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "<p>Harika bir fırsat.</p>", Guid.NewGuid(), [], [], [], [], [], createdAtUtc);
        job.Submit();
        job.Approve(Guid.NewGuid(), publishedAtUtc);
        return job;
    }

    // Bu sınıftaki her test aynı CustomWebApplicationFactory'yi (ve dolayısıyla aynı LocalDB'yi)
    // paylaşıyor (IClassFixture) - tek bir sayfa (örn. page=1, pageSize=50) başka testlerin daha
    // erken PublishedAtUtc'li ilanlarıyla dolup, aranan ilanı daha sonraki bir sayfaya iteleyebilir.
    // Bu yüzden "ilan X sonuçta var mı" türü testler, sıralamadan bağımsız olmak için tüm sayfaları gezer.
    private static async Task<List<PublishedJobSummary>> FetchAllPagesAsync(IPublishedJobModuleContract contract)
    {
        var items = new List<PublishedJobSummary>();
        var page = 1;

        while (true)
        {
            var result = await contract.GetPublishedJobSummariesAsync(page, 200, CancellationToken.None);
            items.AddRange(result.Items);

            if (result.Items.Count == 0 || page >= result.TotalPages)
            {
                break;
            }

            page++;
        }

        return items;
    }

    [Fact]
    public async Task GetPublishedJobSummariesAsync_OnlyReturnsPublishedJobsWithApprovedCompanies()
    {
        using var scope = _factory.Services.CreateScope();
        var companyRepository = scope.ServiceProvider.GetRequiredService<ICompanyRepository>();
        var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(EmployerModuleMarker.UnitOfWorkKey);
        var contract = scope.ServiceProvider.GetRequiredService<IPublishedJobModuleContract>();

        var approvedCompany = CreateApprovedCompany("Acme A.Ş.");
        companyRepository.Add(approvedCompany);
        var publishedJob = CreatePublishedJob(approvedCompany.Id, "Yayındaki İlan", DateTime.UtcNow, DateTime.UtcNow);
        jobRepository.Add(publishedJob);

        var pendingCompany = CreatePendingCompany("Onay Bekleyen A.Ş.");
        companyRepository.Add(pendingCompany);
        var jobUnderUnapprovedCompany = CreatePublishedJob(pendingCompany.Id, "Onaysız Firma İlanı", DateTime.UtcNow, DateTime.UtcNow);
        jobRepository.Add(jobUnderUnapprovedCompany);

        var draftJobCompany = CreateApprovedCompany("Taslak İlan A.Ş.");
        companyRepository.Add(draftJobCompany);
        var draftJob = Job.Create(
            draftJobCompany.Id, "Taslak İlan", false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "<p>Açıklama</p>", Guid.NewGuid(), [], [], [], [], [], DateTime.UtcNow);
        jobRepository.Add(draftJob);

        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var allItems = await FetchAllPagesAsync(contract);

        Assert.Contains(allItems, i => i.JobId == publishedJob.Id);
        Assert.DoesNotContain(allItems, i => i.JobId == jobUnderUnapprovedCompany.Id);
        Assert.DoesNotContain(allItems, i => i.JobId == draftJob.Id);

        var summary = allItems.Single(i => i.JobId == publishedJob.Id);
        Assert.Equal(publishedJob.Slug, summary.Slug);
        Assert.Equal("Acme A.Ş.", summary.CompanyName);
        Assert.Equal("Harika bir fırsat.", summary.Summary);
    }

    [Fact]
    public async Task GetPublishedJobSummariesAsync_WithPageSizeAboveMaximum_ClampsTo200()
    {
        using var scope = _factory.Services.CreateScope();
        var contract = scope.ServiceProvider.GetRequiredService<IPublishedJobModuleContract>();

        var result = await contract.GetPublishedJobSummariesAsync(1, 500, CancellationToken.None);

        Assert.Equal(200, result.PageSize);
    }

    [Fact]
    public async Task GetPublishedJobSummariesAsync_WithPageBelowOne_NormalizesToPage1()
    {
        using var scope = _factory.Services.CreateScope();
        var contract = scope.ServiceProvider.GetRequiredService<IPublishedJobModuleContract>();

        var result = await contract.GetPublishedJobSummariesAsync(0, 10, CancellationToken.None);

        Assert.Equal(1, result.Page);
    }

    // Master prompt: "sayfalar arası kayıpsız/çiftsiz gezinme (250 ilan, sayfa 100 → toplam eşleşmesi)".
    [Fact]
    public async Task GetPublishedJobSummariesAsync_PagesThrough250Jobs_WithoutGapsOrDuplicates()
    {
        using var scope = _factory.Services.CreateScope();
        var companyRepository = scope.ServiceProvider.GetRequiredService<ICompanyRepository>();
        var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(EmployerModuleMarker.UnitOfWorkKey);
        var contract = scope.ServiceProvider.GetRequiredService<IPublishedJobModuleContract>();

        var company = CreateApprovedCompany("Toplu İlan A.Ş.");
        companyRepository.Add(company);

        const int jobCount = 250;
        var baseTime = DateTime.UtcNow.AddDays(-1);
        var createdJobIds = new List<Guid>();
        for (var i = 0; i < jobCount; i++)
        {
            var job = CreatePublishedJob(company.Id, $"Toplu İlan {i}", baseTime, baseTime.AddSeconds(i));
            jobRepository.Add(job);
            createdJobIds.Add(job.Id);
        }

        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var seenJobIds = new List<Guid>();
        for (var pageNumber = 1; pageNumber <= 3; pageNumber++)
        {
            var page = await contract.GetPublishedJobSummariesAsync(pageNumber, 100, CancellationToken.None);
            seenJobIds.AddRange(page.Items.Select(i => i.JobId));
        }

        var seenCreatedJobIds = seenJobIds.Where(createdJobIds.Contains).ToList();
        Assert.Equal(jobCount, seenCreatedJobIds.Distinct().Count());
        Assert.Equal(seenCreatedJobIds.Count, seenCreatedJobIds.Distinct().Count());
    }

    [Fact]
    public async Task GetPublishedJobSummariesAsync_OrdersByPublishedAtAscending_ThenById()
    {
        using var scope = _factory.Services.CreateScope();
        var companyRepository = scope.ServiceProvider.GetRequiredService<ICompanyRepository>();
        var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(EmployerModuleMarker.UnitOfWorkKey);
        var contract = scope.ServiceProvider.GetRequiredService<IPublishedJobModuleContract>();

        var company = CreateApprovedCompany("Sıralama A.Ş.");
        companyRepository.Add(company);

        var baseTime = DateTime.UtcNow.AddDays(-2);
        var older = CreatePublishedJob(company.Id, "Eski İlan", baseTime, baseTime);
        var newer = CreatePublishedJob(company.Id, "Yeni İlan", baseTime, baseTime.AddMinutes(5));
        jobRepository.Add(older);
        jobRepository.Add(newer);

        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var result = await contract.GetPublishedJobSummariesAsync(1, 200, CancellationToken.None);

        var olderIndex = result.Items.ToList().FindIndex(i => i.JobId == older.Id);
        var newerIndex = result.Items.ToList().FindIndex(i => i.JobId == newer.Id);

        Assert.True(olderIndex >= 0 && newerIndex >= 0);
        Assert.True(olderIndex < newerIndex);
    }
}
