using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.Modules.Identity.Features.GetCurrentUser;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Identity.Features.RegisterUser;

namespace GenclikMerkezi.IntegrationTests.Identity;

public class AuthenticationFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthenticationFlowTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RegisterLoginRefreshLogout_FullFlow_BehavesCorrectly()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email, password = "Sifre123", firstName = "Test", lastName = "User", role = "Candidate" });
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var registered = await registerResponse.Content.ReadFromJsonAsync<RegisterUserResponse>();
        Assert.Equal(email, registered!.Email);

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/login", new { email, password = "Sifre123" });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login);

        var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/me");
        meRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
        var meResponse = await _client.SendAsync(meRequest);
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        var me = await meResponse.Content.ReadFromJsonAsync<GetCurrentUserResponse>();
        Assert.Equal(email, me!.Email);

        var unauthorizedMe = await _client.GetAsync("/api/v1/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedMe.StatusCode);

        var refreshResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/refresh-token", new { refreshToken = login.RefreshToken });
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        var refreshed = await refreshResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(refreshed);
        Assert.NotEqual(login.RefreshToken, refreshed!.RefreshToken);

        var reuseResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/refresh-token", new { refreshToken = login.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, reuseResponse.StatusCode);

        var postReuseRefreshResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/refresh-token", new { refreshToken = refreshed!.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, postReuseRefreshResponse.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesRefreshToken_SoItCannotBeUsedAgain()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email, password = "Sifre123", firstName = "Test", lastName = "User", role = "Candidate" });
        var loginResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/login", new { email, password = "Sifre123" });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var logoutRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout")
        {
            Content = JsonContent.Create(new { refreshToken = login!.RefreshToken }),
        };
        logoutRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
        var logoutResponse = await _client.SendAsync(logoutRequest);
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        var refreshAfterLogout = await _client.PostAsJsonAsync(
            "/api/v1/auth/refresh-token", new { refreshToken = login.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, refreshAfterLogout.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email, password = "Sifre123", firstName = "Test", lastName = "User", role = "Candidate" });

        var duplicateResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email, password = "Sifre123", firstName = "Test", lastName = "User", role = "Candidate" });

        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task Register_WithAdminRole_IsRejectedByValidation()
    {
        var email = $"hacker-{Guid.NewGuid():N}@example.com";

        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email, password = "Sifre123", firstName = "Test", lastName = "User", role = "Admin" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
