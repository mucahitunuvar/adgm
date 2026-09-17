using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.Modules.Identity.Features.AdminGetUsers;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Identity.Features.RegisterUser;

namespace GenclikMerkezi.IntegrationTests.Identity;

public class AdminGetUsersFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AdminGetUsersFlowTests(CustomWebApplicationFactory factory)
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

    [Fact]
    public async Task GetUsers_AsAdmin_ReturnsRegisteredUsersFilteredByRoleAndEmailSubstring()
    {
        var accessToken = await LoginAsAdminAsync();
        var uniqueSuffix = Guid.NewGuid().ToString("N");
        var candidateEmail = $"aday-{uniqueSuffix}@example.com";
        var employerEmail = $"employer-{uniqueSuffix}@example.com";

        await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email = candidateEmail, password = "Sifre123", role = "Candidate" });
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email = employerEmail, password = "Sifre123", role = "Employer" });

        var request = new HttpRequestMessage(
            HttpMethod.Get, $"/api/v1/auth/admin/users?email={uniqueSuffix}&role=Candidate");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AdminGetUsersResponse>();
        Assert.NotNull(body);
        Assert.Equal(1, body!.TotalCount);
        Assert.Equal(candidateEmail, body.Items[0].Email);
        Assert.Equal("Candidate", body.Items[0].Role);
    }

    [Fact]
    public async Task GetUsers_AsNonAdmin_ReturnsForbidden()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email, password = "Sifre123", role = "Candidate" });
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Sifre123" });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/admin/users");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/auth/admin/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // PagedRequest (SharedKernel) clamps out-of-range Page/PageSize instead of failing validation
    // (ARCHITECTURE.md §9 "Pagination Convention") - pageSize=0 is silently clamped to 1, not a 400.
    [Fact]
    public async Task GetUsers_WithOutOfRangePageSize_ClampsInsteadOfBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/admin/users?pageSize=0");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AdminGetUsersResponse>();
        Assert.Equal(1, body!.PageSize);
    }

    [Fact]
    public async Task GetUsers_WithInvalidRoleName_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/admin/users?role=NotARealRole");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithInvalidStatusName_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/admin/users?status=NotARealStatus");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_FilteredByStatus_ReturnsOnlyMatchingUsers()
    {
        var accessToken = await LoginAsAdminAsync();
        var uniqueSuffix = Guid.NewGuid().ToString("N");
        var candidateEmail = $"aday-{uniqueSuffix}@example.com";

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email = candidateEmail, password = "Sifre123", role = "Candidate" });
        var registered = await registerResponse.Content.ReadFromJsonAsync<RegisterUserResponse>();

        var deactivateRequest = new HttpRequestMessage(
            HttpMethod.Put, $"/api/v1/auth/admin/users/{registered!.UserId}/deactivate");
        deactivateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        await _client.SendAsync(deactivateRequest);

        var request = new HttpRequestMessage(
            HttpMethod.Get, $"/api/v1/auth/admin/users?email={uniqueSuffix}&status=Disabled");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<AdminGetUsersResponse>();

        Assert.Equal(1, body!.TotalCount);
        Assert.Equal("Disabled", body.Items[0].Status);
    }
}
