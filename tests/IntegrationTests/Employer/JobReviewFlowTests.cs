using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using GenclikMerkezi.Modules.Employer.Features.CreateJob;
using GenclikMerkezi.Modules.Employer.Features.RegisterEmployer;
using GenclikMerkezi.Modules.Identity.Features.Login;

namespace GenclikMerkezi.IntegrationTests.Employer;

// Submit -> Approve/Reject/RequestRevision uçtan uca akış + yanlış danışmanın review denemesinin
// Forbidden ile reddedildiğini doğrular (Görev 2, kullanıcıyla netleştirilen iş akışı).
public class JobReviewFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public JobReviewFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<Guid> CreateAndSubmitJobAsync(string employerAccessToken)
    {
        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/employer/jobs")
        {
            Content = JsonContent.Create(ValidJobPayload()),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", employerAccessToken);
        var createResponse = await _client.SendAsync(createRequest);
        var createBody = await createResponse.Content.ReadFromJsonAsync<CreateJobResponse>();

        var submitRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/employer/jobs/{createBody!.JobId}/submit");
        submitRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", employerAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(submitRequest)).StatusCode);

        return createBody.JobId;
    }

    [Fact]
    public async Task Approve_AsAssignedAdvisor_PublishesJob()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var advisorAccessToken = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        var (employerAccessToken, _) = await RegisterAndApproveEmployerAsync(adminAccessToken);
        var jobId = await CreateAndSubmitJobAsync(employerAccessToken);

        var approveRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/career-advisor/jobs/{jobId}/approve");
        approveRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);

        var response = await _client.SendAsync(approveRequest);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Reject_AsAssignedAdvisor_RejectsJob()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var advisorAccessToken = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        var (employerAccessToken, _) = await RegisterAndApproveEmployerAsync(adminAccessToken);
        var jobId = await CreateAndSubmitJobAsync(employerAccessToken);

        var rejectRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/career-advisor/jobs/{jobId}/reject")
        {
            Content = JsonContent.Create(new { reason = "Eksik bilgi" }),
        };
        rejectRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);

        var response = await _client.SendAsync(rejectRequest);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RequestRevision_AsAssignedAdvisor_ThenResubmitAndApprove_PublishesJob()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var advisorAccessToken = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        var (employerAccessToken, _) = await RegisterAndApproveEmployerAsync(adminAccessToken);
        var jobId = await CreateAndSubmitJobAsync(employerAccessToken);

        var revisionRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/career-advisor/jobs/{jobId}/request-revision")
        {
            Content = JsonContent.Create(new { notes = "Lütfen açıklamayı detaylandırın" }),
        };
        revisionRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(revisionRequest)).StatusCode);

        // Firma düzeltme sonrası tekrar gönderir (RevisionRequested -> UnderReview).
        var resubmitRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/employer/jobs/{jobId}/submit");
        resubmitRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", employerAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(resubmitRequest)).StatusCode);

        var approveRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/career-advisor/jobs/{jobId}/approve");
        approveRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);

        var response = await _client.SendAsync(approveRequest);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Approve_AsDifferentAdvisor_ReturnsForbidden()
    {
        var adminAccessToken = await LoginAsAdminAsync();

        // En-az-yüklü danışman ataması global (bu test sınıfının paylaştığı tek CustomWebApplicationFactory
        // üzerinde tüm testler arasında) olduğu için, hiçbir zaman bir firmaya atanmayan bir danışman
        // sonraki testler için 0-yük "mayın" bırakır ve onların "kendi az önce yarattığı danışman
        // atanacak" varsayımını bozabilir. Bu yüzden otherAdvisor'a da (kullanılmayacak olsa dahi) bir
        // şirket atanarak yükü 1'e çıkarılıyor - testin kendisi hiçbir sızıntı bırakmıyor.
        var otherAdvisorAccessToken = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        await RegisterAndApproveEmployerAsync(adminAccessToken);

        // Şimdi yeni bir danışman yaratılıyor - otherAdvisor artık 1 yükte olduğu için (ve önceki
        // testlerden kalan tüm danışmanlar da kendi firmalarına atanmış durumda), bu yeni danışman bu
        // noktada tek 0-yüklü danışman ve gerçek firma ona atanacak.
        await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        var (employerAccessToken, _) = await RegisterAndApproveEmployerAsync(adminAccessToken);
        var jobId = await CreateAndSubmitJobAsync(employerAccessToken);

        // Firmaya atanan danışman değil, ayrı bir danışman review denemesi yapıyor.
        var approveRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/career-advisor/jobs/{jobId}/approve");
        approveRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", otherAdvisorAccessToken);

        var response = await _client.SendAsync(approveRequest);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
