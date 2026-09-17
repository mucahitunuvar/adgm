using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Candidate.Features.SearchCandidateCvs;
using GenclikMerkezi.Modules.Identity.Features.Login;

namespace GenclikMerkezi.IntegrationTests.Candidate;

public class SearchCandidateCvsFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SearchCandidateCvsFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SearchCandidateCvs_AsAdmin_ReturnsRegisteredCandidates()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/candidates/register",
            new { email, password = "Sifre123", firstName = "Ahmet", lastName = "Yılmaz", phoneNumber = (string?)null });

        var adminEmail = $"admin-{Guid.NewGuid():N}@example.com";
        const string adminPassword = "AdminSifre123";
        await _factory.SeedAdminUserAsync(adminEmail, adminPassword);
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = adminEmail, password = adminPassword });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/candidates?searchText={Uri.EscapeDataString("Ahmet")}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<SearchCandidateCvsResponse>();
        Assert.Contains(body!.Items, i => i.Email == email);
    }

    [Fact]
    public async Task SearchCandidateCvs_AsCandidate_ReturnsForbidden()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        const string password = "Sifre123";
        await _client.PostAsJsonAsync(
            "/api/v1/candidates/register",
            new { email, password, firstName = "Ahmet", lastName = "Yılmaz", phoneNumber = (string?)null });

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/candidates");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
