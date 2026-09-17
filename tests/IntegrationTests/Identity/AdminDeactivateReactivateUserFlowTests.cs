using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.Modules.Identity.Features.AdminGetUserById;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Identity.Features.RegisterUser;

namespace GenclikMerkezi.IntegrationTests.Identity;

public class AdminDeactivateReactivateUserFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AdminDeactivateReactivateUserFlowTests(CustomWebApplicationFactory factory)
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
    public async Task DeactivateThenReactivate_FullFlow_BlocksThenRestoresLogin()
    {
        var candidateEmail = $"aday-{Guid.NewGuid():N}@example.com";
        var registerResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email = candidateEmail, password = "Sifre123", firstName = "Test", lastName = "User", role = "Candidate" });
        var registered = await registerResponse.Content.ReadFromJsonAsync<RegisterUserResponse>();

        var accessToken = await LoginAsAdminAsync();

        var deactivateRequest = new HttpRequestMessage(
            HttpMethod.Put, $"/api/v1/auth/admin/users/{registered!.UserId}/deactivate");
        deactivateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var deactivateResponse = await _client.SendAsync(deactivateRequest);
        Assert.Equal(HttpStatusCode.NoContent, deactivateResponse.StatusCode);

        var blockedLoginResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/login", new { email = candidateEmail, password = "Sifre123" });
        Assert.Equal(HttpStatusCode.Forbidden, blockedLoginResponse.StatusCode);
        var problem = await blockedLoginResponse.Content.ReadFromJsonAsync<ProblemDetailsBody>();
        Assert.Equal("Auth.AccountDeactivated", problem!.Title);

        var detailRequestAfterDeactivate = new HttpRequestMessage(
            HttpMethod.Get, $"/api/v1/auth/admin/users/{registered.UserId}");
        detailRequestAfterDeactivate.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var detailAfterDeactivate = await (await _client.SendAsync(detailRequestAfterDeactivate))
            .Content.ReadFromJsonAsync<AdminUserDetailResponse>();
        Assert.Equal("Disabled", detailAfterDeactivate!.Status);

        var reactivateRequest = new HttpRequestMessage(
            HttpMethod.Put, $"/api/v1/auth/admin/users/{registered.UserId}/reactivate");
        reactivateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var reactivateResponse = await _client.SendAsync(reactivateRequest);
        Assert.Equal(HttpStatusCode.NoContent, reactivateResponse.StatusCode);

        var restoredLoginResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/login", new { email = candidateEmail, password = "Sifre123" });
        Assert.Equal(HttpStatusCode.OK, restoredLoginResponse.StatusCode);
    }

    [Fact]
    public async Task DeactivateUser_AsNonAdmin_ReturnsForbidden()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email, password = "Sifre123", firstName = "Test", lastName = "User", role = "Candidate" });
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Sifre123" });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/auth/admin/users/{Guid.NewGuid()}/deactivate");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ReactivateUser_WithUnknownUserId_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();

        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/auth/admin/users/{Guid.NewGuid()}/reactivate");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed record ProblemDetailsBody(string Title, string Detail);
}
