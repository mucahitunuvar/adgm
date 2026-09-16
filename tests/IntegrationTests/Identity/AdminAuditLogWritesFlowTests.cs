using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Identity.Features.RegisterUser;

namespace GenclikMerkezi.IntegrationTests.Identity;

// Verifies the ADR-015 mechanism end-to-end against the real database: each admin command
// handler's own SaveChangesAsync commits the state change first, then IdentityDbContext dispatches
// the resulting domain event, whose handler adds an AdminAuditLogEntry and calls
// SaveChangesAsync AGAIN on the same (keyed) IUnitOfWork/DbContext - a nested call, not a
// concurrent one, since everything here is sequentially awaited. This is the one thing worth
// proving against a real SqlServer/LocalDB connection rather than trusting reasoning about it.
public class AdminAuditLogWritesFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AdminAuditLogWritesFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(Guid AdminUserId, string AccessToken)> LoginAsAdminAsync()
    {
        var email = $"admin-{Guid.NewGuid():N}@example.com";
        const string password = "AdminSifre123";
        var adminUserId = await _factory.SeedAdminUserAsync(email, password);

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return (adminUserId, login!.AccessToken);
    }

    private async Task<Guid> RegisterCandidateAsync()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email, password = "Sifre123", role = "Candidate" });
        var registered = await response.Content.ReadFromJsonAsync<RegisterUserResponse>();
        return registered!.UserId;
    }

    [Fact]
    public async Task Unlock_WritesAuditLogEntry()
    {
        var (adminUserId, accessToken) = await LoginAsAdminAsync();
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        var registerResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email, password = "Sifre123", role = "Candidate" });
        var registered = await registerResponse.Content.ReadFromJsonAsync<RegisterUserResponse>();
        var targetUserId = registered!.UserId;

        // ManuallyUnlock is a no-op (no domain event, nothing audited) unless the account is
        // actually locked - lock it first via failed login attempts, same as AdminUnlockUserFlowTests.
        for (var i = 0; i < 5; i++)
        {
            await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "WrongPassword1" });
        }

        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/auth/admin/users/{targetUserId}/unlock");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        await _client.SendAsync(request);

        var entries = await _factory.GetAdminAuditLogEntriesAsync();
        var entry = Assert.Single(entries, e => e.TargetUserId == targetUserId);
        Assert.Equal(adminUserId, entry.AdminUserId);
        Assert.Equal(AdminActionType.ManuallyUnlocked, entry.ActionType);
    }

    [Fact]
    public async Task Deactivate_WritesAuditLogEntry()
    {
        var (adminUserId, accessToken) = await LoginAsAdminAsync();
        var targetUserId = await RegisterCandidateAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Put, $"/api/v1/auth/admin/users/{targetUserId}/deactivate");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        await _client.SendAsync(request);

        var entries = await _factory.GetAdminAuditLogEntriesAsync();
        var entry = Assert.Single(entries, e => e.TargetUserId == targetUserId);
        Assert.Equal(adminUserId, entry.AdminUserId);
        Assert.Equal(AdminActionType.Deactivated, entry.ActionType);
    }

    [Fact]
    public async Task Reactivate_WritesAuditLogEntry()
    {
        var (adminUserId, accessToken) = await LoginAsAdminAsync();
        var targetUserId = await RegisterCandidateAsync();

        var deactivateRequest = new HttpRequestMessage(
            HttpMethod.Put, $"/api/v1/auth/admin/users/{targetUserId}/deactivate");
        deactivateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        await _client.SendAsync(deactivateRequest);

        var reactivateRequest = new HttpRequestMessage(
            HttpMethod.Put, $"/api/v1/auth/admin/users/{targetUserId}/reactivate");
        reactivateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        await _client.SendAsync(reactivateRequest);

        var entries = await _factory.GetAdminAuditLogEntriesAsync();
        var entry = Assert.Single(
            entries, e => e.TargetUserId == targetUserId && e.ActionType == AdminActionType.Reactivated);
        Assert.Equal(adminUserId, entry.AdminUserId);
    }

    [Fact]
    public async Task ChangeUserRole_WritesAuditLogEntryWithPreviousAndNewRoleDetails()
    {
        var (adminUserId, accessToken) = await LoginAsAdminAsync();
        var targetUserId = await RegisterCandidateAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Put, $"/api/v1/auth/admin/users/{targetUserId}/role")
        {
            Content = JsonContent.Create(new { newRole = "Employer" }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        await _client.SendAsync(request);

        var entries = await _factory.GetAdminAuditLogEntriesAsync();
        var entry = Assert.Single(entries, e => e.TargetUserId == targetUserId);
        Assert.Equal(adminUserId, entry.AdminUserId);
        Assert.Equal(AdminActionType.RoleChanged, entry.ActionType);
        Assert.Contains("Candidate", entry.Details);
        Assert.Contains("Employer", entry.Details);
    }

    [Fact]
    public async Task ChangeUserRole_ToSameRole_WritesNoAuditLogEntry()
    {
        var (_, accessToken) = await LoginAsAdminAsync();
        var targetUserId = await RegisterCandidateAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Put, $"/api/v1/auth/admin/users/{targetUserId}/role")
        {
            Content = JsonContent.Create(new { newRole = "Candidate" }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        await _client.SendAsync(request);

        var entries = await _factory.GetAdminAuditLogEntriesAsync();
        Assert.DoesNotContain(entries, e => e.TargetUserId == targetUserId);
    }
}
