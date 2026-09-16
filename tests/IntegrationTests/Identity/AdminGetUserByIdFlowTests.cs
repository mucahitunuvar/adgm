using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.Modules.Identity.Features.AdminGetUserById;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Identity.Features.RegisterUser;

namespace GenclikMerkezi.IntegrationTests.Identity;

public class AdminGetUserByIdFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AdminGetUserByIdFlowTests(CustomWebApplicationFactory factory)
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
    public async Task GetUserById_AsAdmin_ReturnsUserDetail()
    {
        var accessToken = await LoginAsAdminAsync();
        var candidateEmail = $"aday-{Guid.NewGuid():N}@example.com";

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email = candidateEmail, password = "Sifre123", role = "Candidate" });
        var registered = await registerResponse.Content.ReadFromJsonAsync<RegisterUserResponse>();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/auth/admin/users/{registered!.UserId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AdminUserDetailResponse>();
        Assert.NotNull(body);
        Assert.Equal(registered.UserId, body!.UserId);
        Assert.Equal(candidateEmail, body.Email);
        Assert.Equal("Candidate", body.Role);
        Assert.False(body.EmailConfirmed);
        Assert.False(body.IsLockedOut);
        Assert.Equal(0, body.FailedLoginAttemptCount);
    }

    [Fact]
    public async Task GetUserById_WithUnknownUserId_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/auth/admin/users/{Guid.NewGuid()}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_AsNonAdmin_ReturnsForbidden()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email, password = "Sifre123", role = "Candidate" });
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Sifre123" });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/auth/admin/users/{Guid.NewGuid()}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
