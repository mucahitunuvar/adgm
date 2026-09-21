using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using GenclikMerkezi.Modules.Employer.Features.CreateJob;
using GenclikMerkezi.Modules.Employer.Features.GetPublishedJobs;
using GenclikMerkezi.Modules.Employer.Features.RegisterEmployer;
using GenclikMerkezi.Modules.Identity.Features.Login;

namespace GenclikMerkezi.IntegrationTests.Employer;

public class GetPublishedJobsFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GetPublishedJobsFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<string> CreateCareerAdvisorAndLoginAsync(string adminAccessToken)
    {
        var email = $"danisman-{Guid.NewGuid():N}@example.com";
        const string password = "Sifre123";

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/career-advisors")
        {
            Content = JsonContent.Create(new { email, password, firstName = "Ayşe", lastName = "Kaya", phoneNumber = (string?)null }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        await response.Content.ReadFromJsonAsync<CreateCareerAdvisorResponse>();

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return login!.AccessToken;
    }

    private async Task<string> RegisterAndApproveEmployerAsync(string adminAccessToken)
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
        return login!.AccessToken;
    }

    private static object ValidJobPayload() => new
    {
        title = "Kaynakçı",
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

    private async Task<Guid> CreateJobAsync(string employerAccessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/employer/jobs")
        {
            Content = JsonContent.Create(ValidJobPayload()),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", employerAccessToken);
        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<CreateJobResponse>();
        return body!.JobId;
    }

    [Fact]
    public async Task GetPublishedJobs_ReturnsOnlyPublishedJobs_NotDraftOnes()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var advisorAccessToken = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        var employerAccessToken = await RegisterAndApproveEmployerAsync(adminAccessToken);

        // Draft: hiç submit/approve edilmiyor, listede görünmemeli.
        await CreateJobAsync(employerAccessToken);

        // Published: submit + approve edilecek.
        var publishedJobId = await CreateJobAsync(employerAccessToken);
        var submitRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/employer/jobs/{publishedJobId}/submit");
        submitRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", employerAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(submitRequest)).StatusCode);

        var approveRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/career-advisor/jobs/{publishedJobId}/approve");
        approveRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(approveRequest)).StatusCode);

        var response = await _client.GetAsync("/api/v1/jobs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<JobResponse>>();
        Assert.NotNull(body);
        Assert.Contains(body!, j => j.Id == publishedJobId);
        Assert.All(body, j => Assert.Equal(GenclikMerkezi.Modules.Employer.Domain.JobStatus.Published, j.Status));
    }

    [Fact]
    public async Task GetPublishedJobs_Unauthenticated_Succeeds()
    {
        var response = await _client.GetAsync("/api/v1/jobs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
