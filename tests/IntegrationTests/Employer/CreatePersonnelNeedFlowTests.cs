using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Employer.Features.CreatePersonnelNeed;
using GenclikMerkezi.Modules.Employer.Features.RegisterEmployer;
using GenclikMerkezi.Modules.Identity.Features.Login;

namespace GenclikMerkezi.IntegrationTests.Employer;

public class CreatePersonnelNeedFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CreatePersonnelNeedFlowTests(CustomWebApplicationFactory factory)
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

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        return (login!.AccessToken, registerBody!.CompanyId);
    }

    private async Task<string> ApproveCompanyAsync(string adminAccessToken, Guid companyId)
    {
        var approveRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/admin/companies/{companyId}/approve");
        approveRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(approveRequest)).StatusCode);
        return adminAccessToken;
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

    [Fact]
    public async Task Create_WithApprovedCompany_CreatesPersonnelNeedInTaslak()
    {
        var (accessToken, companyId) = await RegisterEmployerAsync();
        var adminAccessToken = await LoginAsAdminAsync();
        await ApproveCompanyAsync(adminAccessToken, companyId);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/employer/personnel-needs")
        {
            Content = JsonContent.Create(ValidPersonnelNeedPayload()),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreatePersonnelNeedResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body!.PersonnelNeedId);
    }

    [Fact]
    public async Task Create_WithPendingApprovalCompany_ReturnsConflict()
    {
        var (accessToken, _) = await RegisterEmployerAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/employer/personnel-needs")
        {
            Content = JsonContent.Create(ValidPersonnelNeedPayload()),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithOwnedTaslakPersonnelNeed_Succeeds()
    {
        var (accessToken, companyId) = await RegisterEmployerAsync();
        var adminAccessToken = await LoginAsAdminAsync();
        await ApproveCompanyAsync(adminAccessToken, companyId);

        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/employer/personnel-needs")
        {
            Content = JsonContent.Create(ValidPersonnelNeedPayload()),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var createResponse = await _client.SendAsync(createRequest);
        var createBody = await createResponse.Content.ReadFromJsonAsync<CreatePersonnelNeedResponse>();

        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/employer/personnel-needs/{createBody!.PersonnelNeedId}")
        {
            Content = JsonContent.Create(ValidPersonnelNeedPayload()),
        };
        updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var updateResponse = await _client.SendAsync(updateRequest);

        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
    }

    [Fact]
    public async Task Create_AsNonEmployer_ReturnsForbidden()
    {
        var candidateEmail = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email = candidateEmail, password = "Sifre123", firstName = "Test", lastName = "User", role = "Candidate" });
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = candidateEmail, password = "Sifre123" });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/employer/personnel-needs")
        {
            Content = JsonContent.Create(ValidPersonnelNeedPayload()),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
