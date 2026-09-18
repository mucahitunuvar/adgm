using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using GenclikMerkezi.Modules.Candidate.Features.GetCandidateCv;
using GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;
using GenclikMerkezi.Modules.Identity.Features.Login;

namespace GenclikMerkezi.IntegrationTests.Candidate;

// Uçtan uca Görev 2 (ADR-022 §2): RegisterCandidateCommand'ın CareerAdvisor modülünün public
// contract'ından aktif danışman listesini çekip en az yüklüyü ataması.
public class CandidateCareerAdvisorAssignmentFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CandidateCareerAdvisorAssignmentFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<Guid> CreateCareerAdvisorAsync(string adminAccessToken)
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

        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<CreateCareerAdvisorResponse>();
        return body!.CareerAdvisorId;
    }

    private async Task<(Guid CandidateCvId, string AccessToken)> RegisterAndLoginCandidateAsync()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        const string password = "Sifre123";

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/v1/candidates/register",
            new { email, password, firstName = "Ahmet", lastName = "Yılmaz", phoneNumber = (string?)null });
        var registered = await registerResponse.Content.ReadFromJsonAsync<RegisterCandidateResponse>();

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        return (registered!.CandidateCvId, login!.AccessToken);
    }

    private async Task<GetCandidateCvResponse> GetCandidateCvAsync(Guid candidateCvId, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/candidates/{candidateCvId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);
        return (await response.Content.ReadFromJsonAsync<GetCandidateCvResponse>())!;
    }

    [Fact]
    public async Task Register_WithNoActiveCareerAdvisors_LeavesCareerAdvisorIdNull()
    {
        var (candidateCvId, accessToken) = await RegisterAndLoginCandidateAsync();

        var candidateCv = await GetCandidateCvAsync(candidateCvId, accessToken);

        Assert.Null(candidateCv.CareerAdvisorId);
    }

    [Fact]
    public async Task Register_WithActiveCareerAdvisors_AssignsTheLeastLoadedOne()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var advisorId = await CreateCareerAdvisorAsync(adminAccessToken);

        var (candidateCvId, accessToken) = await RegisterAndLoginCandidateAsync();

        var candidateCv = await GetCandidateCvAsync(candidateCvId, accessToken);

        Assert.Equal(advisorId, candidateCv.CareerAdvisorId);
    }

    [Fact]
    public async Task Register_TwiceWithTwoAdvisors_AssignsSecondCandidateToTheStillIdleAdvisor()
    {
        var adminAccessToken = await LoginAsAdminAsync();
        var firstAdvisorId = await CreateCareerAdvisorAsync(adminAccessToken);
        var secondAdvisorId = await CreateCareerAdvisorAsync(adminAccessToken);

        var (firstCandidateCvId, firstAccessToken) = await RegisterAndLoginCandidateAsync();
        var firstCandidateCv = await GetCandidateCvAsync(firstCandidateCvId, firstAccessToken);
        var firstAssignedAdvisorId = firstCandidateCv.CareerAdvisorId!.Value;
        var stillIdleAdvisorId = firstAssignedAdvisorId == firstAdvisorId ? secondAdvisorId : firstAdvisorId;

        var (secondCandidateCvId, secondAccessToken) = await RegisterAndLoginCandidateAsync();
        var secondCandidateCv = await GetCandidateCvAsync(secondCandidateCvId, secondAccessToken);

        Assert.Equal(stillIdleAdvisorId, secondCandidateCv.CareerAdvisorId);
    }
}
