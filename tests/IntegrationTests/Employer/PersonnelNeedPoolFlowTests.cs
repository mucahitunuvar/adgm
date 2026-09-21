using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using GenclikMerkezi.Modules.Employer.Features.CreatePersonnelNeed;
using GenclikMerkezi.Modules.Employer.Features.RegisterEmployer;
using GenclikMerkezi.Modules.Identity.Features.Login;

namespace GenclikMerkezi.IntegrationTests.Employer;

// Submit -> Pool uçtan uca akış + yanlış danışmanın pool denemesinin Forbidden ile reddedildiğini
// doğrular (Görev 3, JobReviewFlowTests deseniyle aynı).
public class PersonnelNeedPoolFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PersonnelNeedPoolFlowTests(CustomWebApplicationFactory factory)
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

    private static object ValidPersonnelNeedPayload() => new
    {
        employmentTypeId = Guid.NewGuid(),
        workLocationTypeId = Guid.NewGuid(),
        positionId = Guid.NewGuid(),
        departmentId = Guid.NewGuid(),
        quantity = 3,
        provinceId = Guid.NewGuid(),
        experienceLevelId = Guid.NewGuid(),
        detailsText = "Acil ihtiyaç",
        genderPreferenceIds = Array.Empty<Guid>(),
        militaryStatusPreferenceIds = Array.Empty<Guid>(),
        educationLevelPreferenceIds = Array.Empty<Guid>(),
        drivingLicensePreferenceIds = Array.Empty<Guid>(),
    };

    private async Task<Guid> CreateAndSubmitPersonnelNeedAsync(string employerAccessToken)
    {
        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/employer/personnel-needs")
        {
            Content = JsonContent.Create(ValidPersonnelNeedPayload()),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", employerAccessToken);
        var createResponse = await _client.SendAsync(createRequest);
        var createBody = await createResponse.Content.ReadFromJsonAsync<CreatePersonnelNeedResponse>();

        var submitRequest = new HttpRequestMessage(
            HttpMethod.Post, $"/api/v1/employer/personnel-needs/{createBody!.PersonnelNeedId}/submit");
        submitRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", employerAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(submitRequest)).StatusCode);

        return createBody.PersonnelNeedId;
    }

    [Fact]
    public async Task Pool_AsAssignedAdvisor_MovesToGenelHavuzda()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var advisorAccessToken = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        var employerAccessToken = await RegisterAndApproveEmployerAsync(adminAccessToken);
        var personnelNeedId = await CreateAndSubmitPersonnelNeedAsync(employerAccessToken);

        var poolRequest = new HttpRequestMessage(
            HttpMethod.Post, $"/api/v1/career-advisor/personnel-needs/{personnelNeedId}/pool");
        poolRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);

        var response = await _client.SendAsync(poolRequest);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Pool_AsDifferentAdvisor_ReturnsForbidden()
    {
        var adminAccessToken = await LoginAsAdminAsync();

        // JobReviewFlowTests.Approve_AsDifferentAdvisor_ReturnsForbidden'daki en-az-yüklü-danışman
        // "mayın" önleme deseniyle aynı: otherAdvisor'a da bir şirket atanarak yükü 1'e çıkarılıyor,
        // böylece sonraki gerçek firma kaydı garanti şekilde yeni (0 yüklü) danışmana gidiyor.
        var otherAdvisorAccessToken = await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        await RegisterAndApproveEmployerAsync(adminAccessToken);

        await CreateCareerAdvisorAndLoginAsync(adminAccessToken);
        var employerAccessToken = await RegisterAndApproveEmployerAsync(adminAccessToken);
        var personnelNeedId = await CreateAndSubmitPersonnelNeedAsync(employerAccessToken);

        var poolRequest = new HttpRequestMessage(
            HttpMethod.Post, $"/api/v1/career-advisor/personnel-needs/{personnelNeedId}/pool");
        poolRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", otherAdvisorAccessToken);

        var response = await _client.SendAsync(poolRequest);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
