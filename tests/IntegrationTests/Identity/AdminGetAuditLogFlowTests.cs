using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.Modules.Identity.Features.AdminGetAuditLog;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Identity.Features.RegisterUser;

namespace GenclikMerkezi.IntegrationTests.Identity;

public class AdminGetAuditLogFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AdminGetAuditLogFlowTests(CustomWebApplicationFactory factory)
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
    public async Task GetAuditLog_AfterDeactivate_ReturnsEntryFilteredByTargetUserId()
    {
        var accessToken = await LoginAsAdminAsync();

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { email = $"aday-{Guid.NewGuid():N}@example.com", password = "Sifre123", role = "Candidate" });
        var registered = await registerResponse.Content.ReadFromJsonAsync<RegisterUserResponse>();

        var deactivateRequest = new HttpRequestMessage(
            HttpMethod.Put, $"/api/v1/auth/admin/users/{registered!.UserId}/deactivate");
        deactivateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        await _client.SendAsync(deactivateRequest);

        var auditRequest = new HttpRequestMessage(
            HttpMethod.Get, $"/api/v1/auth/admin/audit-log?targetUserId={registered.UserId}");
        auditRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(auditRequest);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AdminGetAuditLogResponse>();
        Assert.NotNull(body);
        Assert.Equal(1, body!.TotalCount);
        Assert.Equal(registered.UserId, body.Items[0].TargetUserId);
        Assert.Equal("Deactivated", body.Items[0].ActionType);
    }

    [Fact]
    public async Task GetAuditLog_FilteredByActionType_ReturnsOnlyMatchingEntries()
    {
        var accessToken = await LoginAsAdminAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Get, "/api/v1/auth/admin/audit-log?actionType=RoleChanged");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAuditLog_WithInvalidActionType_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Get, "/api/v1/auth/admin/audit-log?actionType=NotARealActionType");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAuditLog_AsNonAdmin_ReturnsForbidden()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email, password = "Sifre123", role = "Candidate" });
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Sifre123" });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/admin/audit-log");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAuditLog_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/auth/admin/audit-log");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
