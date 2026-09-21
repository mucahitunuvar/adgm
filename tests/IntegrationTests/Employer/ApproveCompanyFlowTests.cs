using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Employer.Features.RegisterEmployer;
using GenclikMerkezi.Modules.Identity.Features.Login;

namespace GenclikMerkezi.IntegrationTests.Employer;

public class ApproveCompanyFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ApproveCompanyFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<Guid> RegisterCompanyAsync()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/employer/register",
            new
            {
                email = $"firma-{Guid.NewGuid():N}@example.com",
                password = "Sifre123",
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

        var body = await response.Content.ReadFromJsonAsync<RegisterEmployerResponse>();
        return body!.CompanyId;
    }

    [Fact]
    public async Task Approve_AsAdmin_TransitionsCompanyToApproved()
    {
        var companyId = await RegisterCompanyAsync();
        var accessToken = await LoginAsAdminAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/admin/companies/{companyId}/approve");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Approve_TwiceInARow_SecondCallReturnsConflict()
    {
        var companyId = await RegisterCompanyAsync();
        var accessToken = await LoginAsAdminAsync();

        var first = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/admin/companies/{companyId}/approve");
        first.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(first)).StatusCode);

        var second = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/admin/companies/{companyId}/approve");
        second.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var secondResponse = await _client.SendAsync(second);

        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Approve_AsNonAdmin_ReturnsForbidden()
    {
        var companyId = await RegisterCompanyAsync();

        var employerEmail = $"firma2-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email = employerEmail, password = "Sifre123", firstName = "Test", lastName = "User", role = "Employer" });
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = employerEmail, password = "Sifre123" });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/admin/companies/{companyId}/approve");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Approve_Unauthenticated_ReturnsUnauthorized()
    {
        var companyId = await RegisterCompanyAsync();

        var response = await _client.PostAsync($"/api/v1/admin/companies/{companyId}/approve", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
