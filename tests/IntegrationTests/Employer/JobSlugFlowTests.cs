using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.BackfillJobSlugs;
using GenclikMerkezi.Modules.Employer.Features.CreateJob;
using GenclikMerkezi.Modules.Employer.Features.GetCompany;
using GenclikMerkezi.Modules.Employer.Features.RegisterEmployer;
using GenclikMerkezi.Modules.Employer.Infrastructure;
using GenclikMerkezi.Modules.Identity.Features.Login;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Employer;

// Görev 1 (Employer public jobs master prompt): Job.Slug'ın ilk yayında atanması, sonra değişmemesi,
// benzersiz indeksin gerçek bir SQL Server (LocalDB) kısıtı olarak çalışması ve BackfillJobSlugsCommand'ın
// idempotent olması. JobReviewFlowTests'teki HTTP akış yardımcılarının (RegisterAndApproveEmployerAsync
// vb.) birebir kopyası - repoda bu tür akış testleri arasında paylaşılan bir temel sınıf yok.
public class JobSlugFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private const string CareerAdvisorPassword = "Sifre123";

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public JobSlugFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> LoginAsAdminAsync()
    {
        var email = $"admin-{Guid.NewGuid():N}@example.com";
        const string password = "AdminSifre123";
        await _factory.SeedAdminUserAsync(email, password);

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return login!.AccessToken;
    }

    private async Task<(string AccessToken, Guid CareerAdvisorId)> CreateCareerAdvisorAndLoginAsync(string adminAccessToken)
    {
        var email = $"danisman-{Guid.NewGuid():N}@example.com";

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/career-advisors")
        {
            Content = JsonContent.Create(
                new { email, password = CareerAdvisorPassword, firstName = "Ayşe", lastName = "Kaya", phoneNumber = (string?)null }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<CreateCareerAdvisorResponse>();

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = CareerAdvisorPassword });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return (login!.AccessToken, body!.CareerAdvisorId);
    }

    private async Task<string> LoginAsCareerAdvisorByIdAsync(Guid careerAdvisorId)
    {
        var email = await _factory.GetCareerAdvisorEmailAsync(careerAdvisorId);
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = CareerAdvisorPassword });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return login!.AccessToken;
    }

    private async Task<GetCompanyResponse> GetCompanyAsync(Guid companyId, string adminAccessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/admin/companies/{companyId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.SendAsync(request);
        return (await response.Content.ReadFromJsonAsync<GetCompanyResponse>())!;
    }

    private async Task<(string AccessToken, Guid CompanyId)> RegisterAndApproveEmployerAsync(string adminAccessToken)
    {
        var email = $"firma-{Guid.NewGuid():N}@example.com";
        const string password = "Sifre123";

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/v1/employer/register",
            new
            {
                email,
                password,
                name = "Acme A.Ş.",
                sectorId = Guid.NewGuid(),
                foundedYear = (int?)null,
                employeeCount = (int?)null,
                websiteUrl = (string?)null,
                countryId = Guid.NewGuid(),
                provinceId = Guid.NewGuid(),
                districtId = Guid.NewGuid(),
                address = "Adres",
                aboutHtml = (string?)null,
                contactFirstName = "Ayşe",
                contactLastName = "Kaya",
                contactPhone = "05551234567",
                taxOfficeId = Guid.NewGuid(),
                taxNumber = Random.Shared.NextInt64(1_000_000_000L, 9_999_999_999L).ToString(),
                marketingConsent = false,
            });
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<RegisterEmployerResponse>();

        var approveRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/admin/companies/{registerBody!.CompanyId}/approve");
        approveRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(approveRequest)).StatusCode);

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        return (login!.AccessToken, registerBody.CompanyId);
    }

    private async Task<(string EmployerAccessToken, Guid CompanyId, string AssignedAdvisorAccessToken)>
        RegisterEmployerWithAssignedAdvisorAsync(string adminAccessToken)
    {
        var (createdAdvisorAccessToken, createdAdvisorId) = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        var (employerAccessToken, companyId) = await RegisterAndApproveEmployerAsync(adminAccessToken);

        var company = await GetCompanyAsync(companyId, adminAccessToken);
        var assignedAdvisorId = company.CareerAdvisorId!.Value;

        var assignedAdvisorAccessToken = assignedAdvisorId == createdAdvisorId
            ? createdAdvisorAccessToken
            : await LoginAsCareerAdvisorByIdAsync(assignedAdvisorId);

        return (employerAccessToken, companyId, assignedAdvisorAccessToken);
    }

    private static object ValidJobPayload(string title = "Kaynakçı") => new
    {
        title,
        isForDisabledCandidates = false,
        employmentTypeId = Guid.NewGuid(),
        workLocationTypeId = Guid.NewGuid(),
        positionId = Guid.NewGuid(),
        departmentId = Guid.NewGuid(),
        provinceId = Guid.NewGuid(),
        descriptionHtml = "<p>Açıklama</p>",
        experienceLevelId = Guid.NewGuid(),
        genderPreferenceIds = Array.Empty<Guid>(),
        militaryStatusPreferenceIds = Array.Empty<Guid>(),
        educationLevelPreferenceIds = Array.Empty<Guid>(),
        drivingLicensePreferenceIds = Array.Empty<Guid>(),
        languageRequirements = Array.Empty<object>(),
    };

    private async Task<Guid> CreateJobAsync(string employerAccessToken, string title = "Kaynakçı")
    {
        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/employer/jobs")
        {
            Content = JsonContent.Create(ValidJobPayload(title)),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", employerAccessToken);
        var createResponse = await _client.SendAsync(createRequest);
        var createBody = await createResponse.Content.ReadFromJsonAsync<CreateJobResponse>();
        return createBody!.JobId;
    }

    private async Task<Guid> CreateAndSubmitJobAsync(string employerAccessToken, string title = "Kaynakçı")
    {
        var jobId = await CreateJobAsync(employerAccessToken, title);

        var submitRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/employer/jobs/{jobId}/submit");
        submitRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", employerAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(submitRequest)).StatusCode);

        return jobId;
    }

    private async Task ApproveAsync(Guid jobId, string advisorAccessToken)
    {
        var approveRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/career-advisor/jobs/{jobId}/approve");
        approveRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(approveRequest)).StatusCode);
    }

    [Fact]
    public async Task CreateJob_LeavesSlugNull_WhileDraft()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (employerAccessToken, _, _) = await RegisterEmployerWithAssignedAdvisorAsync(adminAccessToken);
        var jobId = await CreateJobAsync(employerAccessToken);

        var slug = await _factory.GetJobSlugAsync(jobId);

        Assert.Null(slug);
    }

    [Fact]
    public async Task Approve_AssignsSlugMatchingGeneratorFormat_OnFirstPublish()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (employerAccessToken, _, advisorAccessToken) = await RegisterEmployerWithAssignedAdvisorAsync(adminAccessToken);
        var jobId = await CreateAndSubmitJobAsync(employerAccessToken, "Depo Görevlisi");

        await ApproveAsync(jobId, advisorAccessToken);

        var slug = await _factory.GetJobSlugAsync(jobId);
        Assert.Equal(JobSlugGenerator.Generate("Depo Görevlisi", jobId), slug);
    }

    [Fact]
    public async Task Suspend_ThenReinstate_DoesNotChangeSlug()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (employerAccessToken, _, advisorAccessToken) = await RegisterEmployerWithAssignedAdvisorAsync(adminAccessToken);
        var jobId = await CreateAndSubmitJobAsync(employerAccessToken);
        await ApproveAsync(jobId, advisorAccessToken);
        var originalSlug = await _factory.GetJobSlugAsync(jobId);

        var suspendRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/admin/jobs/{jobId}/suspend")
        {
            Content = JsonContent.Create(new { reason = "Uygunsuz içerik" }),
        };
        suspendRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(suspendRequest)).StatusCode);

        var reinstateRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/admin/jobs/{jobId}/reinstate");
        reinstateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(reinstateRequest)).StatusCode);

        var slugAfterReinstate = await _factory.GetJobSlugAsync(jobId);
        Assert.Equal(originalSlug, slugAfterReinstate);
    }

    // Id eki (ilk 8 hex karakter) çakışmayı pratikte imkânsız kıldığı için, gerçek üretim akışı asla
    // aynı Slug'ı iki kez üretmez; bu test, EF Core'un Slug sütunu için JobConfiguration'daki filtreli
    // benzersiz indeksi gerçekten LocalDB'ye uyguladığını - Id çakışmasından bağımsız olarak - doğrudan
    // doğrular (master prompt'un "benzersiz indeks emniyet kemeridir" notu).
    [Fact]
    public async Task Slug_UniqueIndex_RejectsDuplicateNonNullValues()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmployerDbContext>();

        var firstJob = Job.Create(
            Guid.NewGuid(), "İlk İlan", false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "<p>Açıklama</p>", Guid.NewGuid(), [], [], [], [], [], DateTime.UtcNow);
        var secondJob = Job.Create(
            Guid.NewGuid(), "İkinci İlan", false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "<p>Açıklama</p>", Guid.NewGuid(), [], [], [], [], [], DateTime.UtcNow);

        dbContext.Jobs.Add(firstJob);
        dbContext.Jobs.Add(secondJob);
        dbContext.Entry(firstJob).Property(nameof(Job.Slug)).CurrentValue = "duplicate-slug";
        dbContext.Entry(secondJob).Property(nameof(Job.Slug)).CurrentValue = "duplicate-slug";

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task BackfillJobSlugs_AssignsMissingSlugs_AndIsIdempotentOnSecondRun()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (employerAccessToken, _, advisorAccessToken) = await RegisterEmployerWithAssignedAdvisorAsync(adminAccessToken);
        var jobId = await CreateAndSubmitJobAsync(employerAccessToken, "Geçmiş İlan");
        await ApproveAsync(jobId, advisorAccessToken);

        // Slug sütunu eklenmeden önce yayınlanmış geçmiş bir kaydı simüle eder (Approve() artık Slug'ı
        // her zaman atadığı için bu durum normal akışla üretilemez).
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<EmployerDbContext>();
            var job = await dbContext.Jobs.FirstAsync(j => j.Id == jobId);
            dbContext.Entry(job).Property(nameof(Job.Slug)).CurrentValue = null;
            await dbContext.SaveChangesAsync();
        }

        Assert.Null(await _factory.GetJobSlugAsync(jobId));

        var firstBackfillRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/jobs/backfill-slugs");
        firstBackfillRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var firstResponse = await _client.SendAsync(firstBackfillRequest);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        var firstBody = await firstResponse.Content.ReadFromJsonAsync<BackfillJobSlugsResponse>();
        Assert.True(firstBody!.UpdatedJobCount >= 1);

        var slugAfterBackfill = await _factory.GetJobSlugAsync(jobId);
        Assert.Equal(JobSlugGenerator.Generate("Geçmiş İlan", jobId), slugAfterBackfill);

        var secondBackfillRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/jobs/backfill-slugs");
        secondBackfillRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var secondResponse = await _client.SendAsync(secondBackfillRequest);
        var secondBody = await secondResponse.Content.ReadFromJsonAsync<BackfillJobSlugsResponse>();

        Assert.Equal(0, secondBody!.UpdatedJobCount);
        Assert.Equal(slugAfterBackfill, await _factory.GetJobSlugAsync(jobId));
    }
}
