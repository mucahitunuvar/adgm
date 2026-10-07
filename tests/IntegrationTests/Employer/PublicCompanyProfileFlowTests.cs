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

// Görev 2 (Employer public jobs master prompt): public firma profili, consent-tabanlı logo servisi ve
// RowVersion ile korunan görünürlük anahtarı. JobReviewFlowTests/JobSlugFlowTests'teki HTTP akış
// yardımcılarının kopyası - repoda bu tür akış testleri arasında paylaşılan bir temel sınıf yok.
public class PublicCompanyProfileFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private const string CareerAdvisorPassword = "Sifre123";

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PublicCompanyProfileFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<(string AccessToken, Guid CompanyId)> RegisterEmployerAsync()
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
                foundedYear = (int?)2015,
                employeeCount = (int?)42,
                websiteUrl = "https://acme.example.com",
                countryId = Guid.NewGuid(),
                provinceId = Guid.NewGuid(),
                districtId = Guid.NewGuid(),
                address = "Adres",
                aboutHtml = "<p>Hakkımızda</p><script>alert(1)</script>",
                contactFirstName = "Ayşe",
                contactLastName = "Kaya",
                contactPhone = "05551234567",
                taxOfficeId = Guid.NewGuid(),
                taxNumber = Random.Shared.NextInt64(1_000_000_000L, 9_999_999_999L).ToString(),
                marketingConsent = false,
            });
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<RegisterEmployerResponse>();

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        return (login!.AccessToken, registerBody!.CompanyId);
    }

    private async Task<(string AccessToken, Guid CompanyId)> RegisterAndApproveEmployerAsync(string adminAccessToken)
    {
        var (employerAccessToken, companyId) = await RegisterEmployerAsync();

        var approveRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/admin/companies/{companyId}/approve");
        approveRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(approveRequest)).StatusCode);

        return (employerAccessToken, companyId);
    }

    private async Task<string> UploadLogoAsync(string employerAccessToken, Guid companyId)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent([1, 2, 3, 4]);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "logo.png");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/employer/companies/{companyId}/logo") { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", employerAccessToken);
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return employerAccessToken;
    }

    private async Task<HttpResponseMessage> SetLogoVisibilityAsync(string employerAccessToken, bool showLogoOnWebsite, byte[] rowVersion)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, "/api/v1/employer/companies/me/logo-visibility")
        {
            Content = JsonContent.Create(new { showLogoOnWebsite, rowVersion }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", employerAccessToken);
        return await _client.SendAsync(request);
    }

    private async Task<GetCompanyResponse> GetMyCompanyAsync(string employerAccessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/employer/my-company");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", employerAccessToken);
        var response = await _client.SendAsync(request);
        return (await response.Content.ReadFromJsonAsync<GetCompanyResponse>())!;
    }

    [Fact]
    public async Task GetPublicCompanyProfile_WithPendingApprovalCompany_ReturnsNotFound()
    {
        var (_, companyId) = await RegisterEmployerAsync();

        var response = await _client.GetAsync($"/api/v1/public/companies/{companyId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicCompanyProfile_WithUnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/v1/public/companies/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicCompanyProfile_WithDeactivatedCompany_ReturnsNotFound()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (_, companyId) = await RegisterAndApproveEmployerAsync(adminAccessToken);

        var deactivateRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/admin/companies/{companyId}/deactivate");
        deactivateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(deactivateRequest)).StatusCode);

        var response = await _client.GetAsync($"/api/v1/public/companies/{companyId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicCompanyProfile_WithApprovedCompany_ReturnsSanitizedProfile_WithoutForbiddenFields()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (_, companyId) = await RegisterAndApproveEmployerAsync(adminAccessToken);

        var response = await _client.GetAsync($"/api/v1/public/companies/{companyId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(companyId, json.GetProperty("id").GetGuid());
        Assert.Equal("Acme A.Ş.", json.GetProperty("name").GetString());
        Assert.Equal(2015, json.GetProperty("foundedYear").GetInt32());
        Assert.Equal(42, json.GetProperty("employeeCount").GetInt32());
        Assert.Equal(0, json.GetProperty("publishedJobCount").GetInt32());
        Assert.False(json.GetProperty("hasLogo").GetBoolean());

        // Sanitize edilmiş aboutHtml: script kaldırılmış, güvenli metin kalmış.
        var aboutHtml = json.GetProperty("aboutHtml").GetString();
        Assert.DoesNotContain("<script", aboutHtml, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Hakkımızda", aboutHtml);

        string[] forbiddenProperties =
        [
            "contactFirstName", "contactLastName", "contactEmail", "contactPhone", "taxOfficeId", "taxNumber",
            "careerAdvisorId", "address", "districtId", "countryId", "userId", "status", "marketingConsent",
            "rejectionReason", "approvedByUserId", "approvedAtUtc", "deactivatedByUserId", "deactivatedAtUtc",
            "rowVersion",
        ];
        foreach (var property in forbiddenProperties)
        {
            Assert.False(json.TryGetProperty(property, out _), $"Public response unexpectedly contains '{property}'.");
        }
    }

    [Fact]
    public async Task SetCompanyLogoVisibility_WithoutLogo_ReturnsBadRequest()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (employerAccessToken, _) = await RegisterAndApproveEmployerAsync(adminAccessToken);
        var myCompany = await GetMyCompanyAsync(employerAccessToken);

        var response = await SetLogoVisibilityAsync(employerAccessToken, true, myCompany.RowVersion);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetCompanyLogoVisibility_WithValidLogoAndRowVersion_EnablesVisibility()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (employerAccessToken, companyId) = await RegisterAndApproveEmployerAsync(adminAccessToken);
        await UploadLogoAsync(employerAccessToken, companyId);
        var myCompany = await GetMyCompanyAsync(employerAccessToken);

        var response = await SetLogoVisibilityAsync(employerAccessToken, true, myCompany.RowVersion);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var updatedCompany = await GetMyCompanyAsync(employerAccessToken);
        Assert.True(updatedCompany.ShowLogoOnWebsite);
    }

    [Fact]
    public async Task SetCompanyLogoVisibility_WithStaleRowVersion_ReturnsConflict()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (employerAccessToken, companyId) = await RegisterAndApproveEmployerAsync(adminAccessToken);
        await UploadLogoAsync(employerAccessToken, companyId);
        var staleCompany = await GetMyCompanyAsync(employerAccessToken);

        Assert.Equal(HttpStatusCode.NoContent, (await SetLogoVisibilityAsync(employerAccessToken, true, staleCompany.RowVersion)).StatusCode);

        var response = await SetLogoVisibilityAsync(employerAccessToken, false, staleCompany.RowVersion);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicCompanyLogo_BeforeConsent_ReturnsNotFound()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (employerAccessToken, companyId) = await RegisterAndApproveEmployerAsync(adminAccessToken);
        await UploadLogoAsync(employerAccessToken, companyId);

        var response = await _client.GetAsync($"/api/v1/public/companies/{companyId}/logo");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPublicCompanyLogo_AfterConsent_ReturnsFileBytesWithCacheControl()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (employerAccessToken, companyId) = await RegisterAndApproveEmployerAsync(adminAccessToken);
        await UploadLogoAsync(employerAccessToken, companyId);
        var myCompany = await GetMyCompanyAsync(employerAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await SetLogoVisibilityAsync(employerAccessToken, true, myCompany.RowVersion)).StatusCode);

        var response = await _client.GetAsync($"/api/v1/public/companies/{companyId}/logo");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, await response.Content.ReadAsByteArrayAsync());
        Assert.Equal("public, max-age=3600", response.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task GetPublicCompanyProfile_PublishedJobCount_OnlyCountsPublishedJobs()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var (createdAdvisorAccessToken, createdAdvisorId) = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        var (employerAccessToken, companyId) = await RegisterAndApproveEmployerAsync(adminAccessToken);

        var company = await GetCompanyAsync(companyId, adminAccessToken);
        var assignedAdvisorId = company.CareerAdvisorId!.Value;
        var advisorAccessToken = assignedAdvisorId == createdAdvisorId
            ? createdAdvisorAccessToken
            : await LoginAsCareerAdvisorByIdAsync(assignedAdvisorId);

        var publishedJobCreateRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/employer/jobs")
        {
            Content = JsonContent.Create(new
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
            }),
        };
        publishedJobCreateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", employerAccessToken);
        var publishedJobCreateResponse = await _client.SendAsync(publishedJobCreateRequest);
        var publishedJobBody = await publishedJobCreateResponse.Content.ReadFromJsonAsync<CreateJobResponse>();

        var submitRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/employer/jobs/{publishedJobBody!.JobId}/submit");
        submitRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", employerAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(submitRequest)).StatusCode);

        var approveRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/career-advisor/jobs/{publishedJobBody.JobId}/approve");
        approveRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(approveRequest)).StatusCode);

        // İkinci ilan Draft'ta kalır - sayıma girmemeli.
        var draftJobCreateRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/employer/jobs")
        {
            Content = JsonContent.Create(new
            {
                title = "Depo Görevlisi",
                isForDisabledCandidates = false,
                employmentTypeId = Guid.NewGuid(),
                workLocationTypeId = Guid.NewGuid(),
                positionId = Guid.NewGuid(),
                departmentId = Guid.NewGuid(),
                provinceId = Guid.NewGuid(),
                descriptionHtml = (string?)null,
                experienceLevelId = Guid.NewGuid(),
                genderPreferenceIds = Array.Empty<Guid>(),
                militaryStatusPreferenceIds = Array.Empty<Guid>(),
                educationLevelPreferenceIds = Array.Empty<Guid>(),
                drivingLicensePreferenceIds = Array.Empty<Guid>(),
                languageRequirements = Array.Empty<object>(),
            }),
        };
        draftJobCreateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", employerAccessToken);
        Assert.Equal(HttpStatusCode.Created, (await _client.SendAsync(draftJobCreateRequest)).StatusCode);

        var response = await _client.GetAsync($"/api/v1/public/companies/{companyId}");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(1, json.GetProperty("publishedJobCount").GetInt32());
    }
}
