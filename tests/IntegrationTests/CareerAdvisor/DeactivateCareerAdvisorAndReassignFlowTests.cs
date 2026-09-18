using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using GenclikMerkezi.Modules.Candidate.Features.GetCandidateCv;
using GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;
using GenclikMerkezi.Modules.Identity.Features.Login;

namespace GenclikMerkezi.IntegrationTests.CareerAdvisor;

// Görev 3/ADR-022 §1: danışman deaktivasyonu artık yalnızca Host'taki
// CareerAdvisorDeactivationOrchestrator üzerinden, POST /api/v1/admin/career-advisors/{id}/deactivate
// ile ulaşılabiliyor (Görev 1'in doğrudan PUT endpoint'i kaldırıldı - çıplak deaktivasyon adayları
// öksüz bırakırdı). Bu testler hem deaktivasyonun kendisini hem de yeniden atama davranışını kapsıyor.
public class DeactivateCareerAdvisorAndReassignFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DeactivateCareerAdvisorAndReassignFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static async Task<string> LoginAsAdminAsync(CustomWebApplicationFactory factory, HttpClient client)
    {
        var email = $"admin-{Guid.NewGuid():N}@example.com";
        const string password = "AdminSifre123";
        await factory.SeedAdminUserAsync(email, password);

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return login!.AccessToken;
    }

    private static async Task<Guid> CreateCareerAdvisorAsync(HttpClient client, string adminAccessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/career-advisors")
        {
            Content = JsonContent.Create(new
            {
                email = $"danisman-{Guid.NewGuid():N}@example.com",
                password = "Sifre123",
                firstName = "Ayşe",
                lastName = "Kaya",
                phoneNumber = (string?)null,
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<CreateCareerAdvisorResponse>();
        return body!.CareerAdvisorId;
    }

    private static async Task<HttpResponseMessage> DeactivateCareerAdvisorAsync(HttpClient client, Guid careerAdvisorId, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/admin/career-advisors/{careerAdvisorId}/deactivate");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return await client.SendAsync(request);
    }

    private static async Task<(Guid CandidateCvId, string AccessToken)> RegisterAndLoginCandidateAsync(HttpClient client)
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        const string password = "Sifre123";

        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/candidates/register",
            new { email, password, firstName = "Ahmet", lastName = "Yılmaz", phoneNumber = (string?)null });
        var registered = await registerResponse.Content.ReadFromJsonAsync<RegisterCandidateResponse>();

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        return (registered!.CandidateCvId, login!.AccessToken);
    }

    private static async Task<GetCandidateCvResponse> GetCandidateCvAsync(HttpClient client, Guid candidateCvId, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/candidates/{candidateCvId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.SendAsync(request);
        return (await response.Content.ReadFromJsonAsync<GetCandidateCvResponse>())!;
    }

    [Fact]
    public async Task Deactivate_ThenReDeactivate_IsIdempotent()
    {
        var accessToken = await LoginAsAdminAsync(_factory, _client);
        var careerAdvisorId = await CreateCareerAdvisorAsync(_client, accessToken);

        var firstResponse = await DeactivateCareerAdvisorAsync(_client, careerAdvisorId, accessToken);
        Assert.Equal(HttpStatusCode.NoContent, firstResponse.StatusCode);

        var secondResponse = await DeactivateCareerAdvisorAsync(_client, careerAdvisorId, accessToken);
        Assert.Equal(HttpStatusCode.NoContent, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Deactivate_WithUnknownId_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync(_factory, _client);

        var response = await DeactivateCareerAdvisorAsync(_client, Guid.NewGuid(), accessToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_AsNonAdmin_ReturnsForbidden()
    {
        var accessToken = await LoginAsAdminAsync(_factory, _client);
        var careerAdvisorId = await CreateCareerAdvisorAsync(_client, accessToken);

        var candidateEmail = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email = candidateEmail, password = "Sifre123", firstName = "Test", lastName = "User", role = "Candidate" });
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = candidateEmail, password = "Sifre123" });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var response = await DeactivateCareerAdvisorAsync(_client, careerAdvisorId, login!.AccessToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // Bu iki test, sınıf genelinde paylaşılan CustomWebApplicationFactory'yi (diğer test metotlarının
    // da danışman oluşturup deaktive etmeden bıraktığı bir veritabanını) DEĞİL, kendi özel/izole
    // factory'lerini kullanıyor - "aktif danışman sayısı tam olarak N" varsayımı, sınıf genelinde
    // paylaşılan durumla güvenilir şekilde kurulamaz (xUnit test sırası garantili değil).

    [Fact]
    public async Task Deactivate_WithAnotherActiveAdvisor_ReassignsOrphanedCandidateToIt()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var adminAccessToken = await LoginAsAdminAsync(factory, client);
        var firstAdvisorId = await CreateCareerAdvisorAsync(client, adminAccessToken);
        var secondAdvisorId = await CreateCareerAdvisorAsync(client, adminAccessToken);

        var (candidateCvId, candidateAccessToken) = await RegisterAndLoginCandidateAsync(client);
        var beforeDeactivation = await GetCandidateCvAsync(client, candidateCvId, candidateAccessToken);
        var assignedAdvisorId = beforeDeactivation.CareerAdvisorId!.Value;
        var otherAdvisorId = assignedAdvisorId == firstAdvisorId ? secondAdvisorId : firstAdvisorId;

        var deactivateResponse = await DeactivateCareerAdvisorAsync(client, assignedAdvisorId, adminAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, deactivateResponse.StatusCode);

        var afterDeactivation = await GetCandidateCvAsync(client, candidateCvId, candidateAccessToken);
        Assert.Equal(otherAdvisorId, afterDeactivation.CareerAdvisorId);
    }

    [Fact]
    public async Task Deactivate_TheOnlyActiveAdvisor_LeavesOrphanedCandidateCareerAdvisorIdNull()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var adminAccessToken = await LoginAsAdminAsync(factory, client);
        var onlyAdvisorId = await CreateCareerAdvisorAsync(client, adminAccessToken);

        var (candidateCvId, candidateAccessToken) = await RegisterAndLoginCandidateAsync(client);
        var beforeDeactivation = await GetCandidateCvAsync(client, candidateCvId, candidateAccessToken);
        Assert.Equal(onlyAdvisorId, beforeDeactivation.CareerAdvisorId);

        var deactivateResponse = await DeactivateCareerAdvisorAsync(client, onlyAdvisorId, adminAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, deactivateResponse.StatusCode);

        var afterDeactivation = await GetCandidateCvAsync(client, candidateCvId, candidateAccessToken);
        Assert.Null(afterDeactivation.CareerAdvisorId);
    }
}
