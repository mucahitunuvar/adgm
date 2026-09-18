using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using GenclikMerkezi.Modules.Identity.Features.Login;

namespace GenclikMerkezi.IntegrationTests.CareerAdvisor;

public class CreateCareerAdvisorFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CreateCareerAdvisorFlowTests(CustomWebApplicationFactory factory)
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
    public async Task Create_AsAdmin_CreatesUserAndCareerAdvisor()
    {
        var accessToken = await LoginAsAdminAsync();
        var email = $"danisman-{Guid.NewGuid():N}@example.com";

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/career-advisors")
        {
            Content = JsonContent.Create(new { email, password = "Sifre123", firstName = "Ayşe", lastName = "Kaya", phoneNumber = "05551234567" }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CreateCareerAdvisorResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body!.UserId);
        Assert.NotEqual(Guid.Empty, body.CareerAdvisorId);
    }

    [Fact]
    public async Task Create_AsNonAdmin_ReturnsForbidden()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email, password = "Sifre123", firstName = "Test", lastName = "User", role = "Candidate" });
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Sifre123" });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/career-advisors")
        {
            Content = JsonContent.Create(new { email = $"danisman-{Guid.NewGuid():N}@example.com", password = "Sifre123", firstName = "Ayşe", lastName = "Kaya", phoneNumber = (string?)null }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_Unauthenticated_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/admin/career-advisors",
            new { email = $"danisman-{Guid.NewGuid():N}@example.com", password = "Sifre123", firstName = "Ayşe", lastName = "Kaya", phoneNumber = (string?)null });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
