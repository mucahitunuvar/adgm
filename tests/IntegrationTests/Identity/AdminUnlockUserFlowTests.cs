using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.Modules.Identity.Features.AdminGetUserById;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Identity.Features.RegisterUser;

namespace GenclikMerkezi.IntegrationTests.Identity;

public class AdminUnlockUserFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AdminUnlockUserFlowTests(CustomWebApplicationFactory factory)
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
    public async Task UnlockUser_AfterLockout_AllowsImmediateLoginAgain()
    {
        var candidateEmail = $"aday-{Guid.NewGuid():N}@example.com";
        var registerResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email = candidateEmail, password = "Sifre123", role = "Candidate" });
        var registered = await registerResponse.Content.ReadFromJsonAsync<RegisterUserResponse>();

        for (var i = 0; i < 5; i++)
        {
            await _client.PostAsJsonAsync(
                "/api/v1/auth/login", new { email = candidateEmail, password = "WrongPassword1" });
        }

        var lockedLoginResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/login", new { email = candidateEmail, password = "Sifre123" });
        Assert.Equal(HttpStatusCode.Forbidden, lockedLoginResponse.StatusCode);

        var accessToken = await LoginAsAdminAsync();
        var unlockRequest = new HttpRequestMessage(
            HttpMethod.Put, $"/api/v1/auth/admin/users/{registered!.UserId}/unlock");
        unlockRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var unlockResponse = await _client.SendAsync(unlockRequest);
        Assert.Equal(HttpStatusCode.NoContent, unlockResponse.StatusCode);

        var detailRequest = new HttpRequestMessage(
            HttpMethod.Get, $"/api/v1/auth/admin/users/{registered.UserId}");
        detailRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var detailResponse = await _client.SendAsync(detailRequest);
        var detail = await detailResponse.Content.ReadFromJsonAsync<AdminUserDetailResponse>();
        Assert.False(detail!.IsLockedOut);

        var loginAfterUnlock = await _client.PostAsJsonAsync(
            "/api/v1/auth/login", new { email = candidateEmail, password = "Sifre123" });
        Assert.Equal(HttpStatusCode.OK, loginAfterUnlock.StatusCode);
    }

    [Fact]
    public async Task UnlockUser_AsNonAdmin_ReturnsForbidden()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email, password = "Sifre123", role = "Candidate" });
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Sifre123" });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/auth/admin/users/{Guid.NewGuid()}/unlock");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UnlockUser_WithUnknownUserId_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();

        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/auth/admin/users/{Guid.NewGuid()}/unlock");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
