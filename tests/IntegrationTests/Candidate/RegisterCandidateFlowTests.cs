using System.Net;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;

namespace GenclikMerkezi.IntegrationTests.Candidate;

public class RegisterCandidateFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RegisterCandidateFlowTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidInput_CreatesUserAndCandidateCv()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";

        var response = await _client.PostAsJsonAsync(
            "/api/v1/candidates/register",
            new { email, password = "Sifre123", firstName = "Ahmet", lastName = "Yılmaz", phoneNumber = "05551234567" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<RegisterCandidateResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body!.UserId);
        Assert.NotEqual(Guid.Empty, body.CandidateCvId);
    }

    [Fact]
    public async Task Register_WithAlreadyRegisteredEmail_ReturnsConflict()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        var payload = new { email, password = "Sifre123", firstName = "Ahmet", lastName = "Yılmaz", phoneNumber = (string?)null };

        var first = await _client.PostAsJsonAsync("/api/v1/candidates/register", payload);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await _client.PostAsJsonAsync("/api/v1/candidates/register", payload);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Register_WithEmptyFirstName_ReturnsBadRequest()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";

        var response = await _client.PostAsJsonAsync(
            "/api/v1/candidates/register",
            new { email, password = "Sifre123", firstName = "", lastName = "Yılmaz", phoneNumber = (string?)null });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
