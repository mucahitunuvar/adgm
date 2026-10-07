using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using GenclikMerkezi.Modules.Employer.Features.CreateJob;
using GenclikMerkezi.Modules.Employer.Features.GetCompany;
using GenclikMerkezi.Modules.Employer.Features.RegisterEmployer;
using GenclikMerkezi.Modules.Identity.Features.Login;

namespace GenclikMerkezi.IntegrationTests.Employer;

// Görev 3 (Employer public jobs master prompt): public ilan listesi ve slug tabanlı detay. Önceki
// Employer akış testlerindeki (JobReviewFlowTests, JobSlugFlowTests, PublicCompanyProfileFlowTests)
// HTTP yardımcılarının kopyası.
public class PublicJobsFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private const string CareerAdvisorPassword = "Sifre123";

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PublicJobsFlowTests(CustomWebApplicationFactory factory)
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
        descriptionHtml = "<p>Harika bir ekip arkadaşı arıyoruz.</p><script>alert(1)</script>",
        experienceLevelId = Guid.NewGuid(),
        genderPreferenceIds = Array.Empty<Guid>(),
        militaryStatusPreferenceIds = Array.Empty<Guid>(),
        educationLevelPreferenceIds = Array.Empty<Guid>(),
        drivingLicensePreferenceIds = Array.Empty<Guid>(),
        languageRequirements = Array.Empty<object>(),
    };

    private async Task<(Guid JobId, string Slug)> CreateSubmitAndApproveJobAsync(
        string employerAccessToken, string advisorAccessToken, string title = "Kaynakçı")
    {
        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/employer/jobs")
        {
            Content = JsonContent.Create(ValidJobPayload(title)),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", employerAccessToken);
        var createResponse = await _client.SendAsync(createRequest);
        var createBody = await createResponse.Content.ReadFromJsonAsync<CreateJobResponse>();
        var jobId = createBody!.JobId;

        var submitRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/employer/jobs/{jobId}/submit");
        submitRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", employerAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(submitRequest)).StatusCode);

        var approveRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/career-advisor/jobs/{jobId}/approve");
        approveRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(approveRequest)).StatusCode);

        var slug = await _factory.GetJobSlugAsync(jobId);
        return (jobId, slug!);
    }

    [Fact]
    public async Task GetPublicJobs_ReturnsPublishedJob_WithoutForbiddenFields()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (employerAccessToken, _, advisorAccessToken) = await RegisterEmployerWithAssignedAdvisorAsync(adminAccessToken);
        var (jobId, slug) = await CreateSubmitAndApproveJobAsync(employerAccessToken, advisorAccessToken);

        var response = await _client.GetAsync("/api/v1/public/jobs");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var items = json.GetProperty("items");
        var item = items.EnumerateArray().First(i => i.GetProperty("id").GetGuid() == jobId);

        Assert.Equal(slug, item.GetProperty("slug").GetString());
        Assert.Contains("Harika bir ekip arkadaşı", item.GetProperty("summary").GetString());

        string[] forbiddenProperties = ["genderPreferenceIds", "militaryStatusPreferenceNames", "reviewedByAdvisorId", "rejectionReason", "revisionNotes", "descriptionHtml"];
        foreach (var property in forbiddenProperties)
        {
            Assert.False(item.TryGetProperty(property, out _), $"Public list item unexpectedly contains '{property}'.");
        }
    }

    [Fact]
    public async Task GetPublicJobs_WithQTooShort_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/v1/public/jobs?q=a");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicJobs_WithQTooLong_ReturnsBadRequest()
    {
        var response = await _client.GetAsync($"/api/v1/public/jobs?q={new string('a', 101)}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicJobs_PageSizeIsClampedToMaximum()
    {
        var response = await _client.GetAsync("/api/v1/public/jobs?pageSize=500");

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(100, json.GetProperty("pageSize").GetInt32());
    }

    [Fact]
    public async Task GetPublicJobDetail_WithValidSlug_ReturnsFullDetail_WithoutForbiddenFields()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (employerAccessToken, _, advisorAccessToken) = await RegisterEmployerWithAssignedAdvisorAsync(adminAccessToken);
        var (_, slug) = await CreateSubmitAndApproveJobAsync(employerAccessToken, advisorAccessToken);

        var response = await _client.GetAsync($"/api/v1/public/jobs/{slug}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(slug, json.GetProperty("slug").GetString());

        var descriptionHtml = json.GetProperty("descriptionHtml").GetString();
        Assert.DoesNotContain("<script", descriptionHtml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Harika bir ekip arkadaşı", descriptionHtml);

        string[] forbiddenProperties = ["genderPreferenceIds", "reviewedByAdvisorId", "rejectionReason", "revisionNotes"];
        foreach (var property in forbiddenProperties)
        {
            Assert.False(json.TryGetProperty(property, out _), $"Public job detail unexpectedly contains '{property}'.");
        }
    }

    [Fact]
    public async Task GetPublicJobDetail_WithUnknownSlug_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/v1/public/jobs/bilinmeyen-ilan-12345678");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicJobDetail_AfterAdminSuspendsJob_DisappearsImmediately()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (employerAccessToken, _, advisorAccessToken) = await RegisterEmployerWithAssignedAdvisorAsync(adminAccessToken);
        var (jobId, slug) = await CreateSubmitAndApproveJobAsync(employerAccessToken, advisorAccessToken);

        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"/api/v1/public/jobs/{slug}")).StatusCode);

        var suspendRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/admin/jobs/{jobId}/suspend")
        {
            Content = JsonContent.Create(new { reason = "Uygunsuz içerik" }),
        };
        suspendRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(suspendRequest)).StatusCode);

        var response = await _client.GetAsync($"/api/v1/public/jobs/{slug}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicJobDetail_AfterCompanyDeactivated_DisappearsImmediately()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (employerAccessToken, companyId, advisorAccessToken) = await RegisterEmployerWithAssignedAdvisorAsync(adminAccessToken);
        var (_, slug) = await CreateSubmitAndApproveJobAsync(employerAccessToken, advisorAccessToken);

        var deactivateRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/admin/companies/{companyId}/deactivate");
        deactivateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(deactivateRequest)).StatusCode);

        var response = await _client.GetAsync($"/api/v1/public/jobs/{slug}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicJobs_FiltersByCompanyId()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (firstEmployerAccessToken, firstCompanyId, firstAdvisorAccessToken) = await RegisterEmployerWithAssignedAdvisorAsync(adminAccessToken);
        var (secondEmployerAccessToken, secondCompanyId, secondAdvisorAccessToken) = await RegisterEmployerWithAssignedAdvisorAsync(adminAccessToken);
        var (firstJobId, _) = await CreateSubmitAndApproveJobAsync(firstEmployerAccessToken, firstAdvisorAccessToken, "İlan Bir");
        await CreateSubmitAndApproveJobAsync(secondEmployerAccessToken, secondAdvisorAccessToken, "İlan İki");

        var response = await _client.GetAsync($"/api/v1/public/jobs?companyId={firstCompanyId}");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var items = json.GetProperty("items").EnumerateArray().ToList();

        Assert.All(items, i => Assert.Equal(firstCompanyId, i.GetProperty("company").GetProperty("id").GetGuid()));
        Assert.Contains(items, i => i.GetProperty("id").GetGuid() == firstJobId);
        Assert.DoesNotContain(items, i => i.GetProperty("company").GetProperty("id").GetGuid() == secondCompanyId);
    }
}
