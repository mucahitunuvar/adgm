using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using GenclikMerkezi.Modules.Identity.Features.Login;

namespace GenclikMerkezi.IntegrationTests.CareerAdvisor;

public class DeactivateCareerAdvisorFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DeactivateCareerAdvisorFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<Guid> CreateCareerAdvisorAsync(string accessToken)
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
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<CreateCareerAdvisorResponse>();
        return body!.CareerAdvisorId;
    }

    [Fact]
    public async Task Deactivate_ThenReDeactivate_IsIdempotent()
    {
        var accessToken = await LoginAsAdminAsync();
        var careerAdvisorId = await CreateCareerAdvisorAsync(accessToken);

        var firstRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/admin/career-advisors/{careerAdvisorId}/deactivate");
        firstRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var firstResponse = await _client.SendAsync(firstRequest);
        Assert.Equal(HttpStatusCode.NoContent, firstResponse.StatusCode);

        var secondRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/admin/career-advisors/{careerAdvisorId}/deactivate");
        secondRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var secondResponse = await _client.SendAsync(secondRequest);
        Assert.Equal(HttpStatusCode.NoContent, secondResponse.StatusCode);
    }

    [Fact]
    public async Task Deactivate_WithUnknownId_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();

        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/admin/career-advisors/{Guid.NewGuid()}/deactivate");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Deactivate_AsNonAdmin_ReturnsForbidden()
    {
        var accessToken = await LoginAsAdminAsync();
        var careerAdvisorId = await CreateCareerAdvisorAsync(accessToken);

        var candidateEmail = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email = candidateEmail, password = "Sifre123", firstName = "Test", lastName = "User", role = "Candidate" });
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = candidateEmail, password = "Sifre123" });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/admin/career-advisors/{careerAdvisorId}/deactivate");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
