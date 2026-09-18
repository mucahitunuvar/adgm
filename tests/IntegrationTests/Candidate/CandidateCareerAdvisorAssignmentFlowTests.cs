using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using GenclikMerkezi.Modules.Candidate.Features.GetCandidateCv;
using GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;
using GenclikMerkezi.Modules.Identity.Features.Login;

namespace GenclikMerkezi.IntegrationTests.Candidate;

// Uçtan uca Görev 2 (ADR-022 §2): RegisterCandidateCommand'ın CareerAdvisor modülünün public
// contract'ından aktif danışman listesini çekip en az yüklüyü ataması. Her test kendi özel/izole
// CustomWebApplicationFactory'sini kullanıyor - "aktif danışman sayısı tam olarak N" varsayımı,
// sınıf genelinde paylaşılan bir factory/veritabanıyla (diğer testlerin deaktive etmeden bıraktığı
// danışmanlar yüzünden) güvenilir şekilde kurulamaz.
public class CandidateCareerAdvisorAssignmentFlowTests
{
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
    public async Task Register_WithNoActiveCareerAdvisors_LeavesCareerAdvisorIdNull()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var (candidateCvId, accessToken) = await RegisterAndLoginCandidateAsync(client);

        var candidateCv = await GetCandidateCvAsync(client, candidateCvId, accessToken);

        Assert.Null(candidateCv.CareerAdvisorId);
    }

    [Fact]
    public async Task Register_WithActiveCareerAdvisors_AssignsTheLeastLoadedOne()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var adminAccessToken = await LoginAsAdminAsync(factory, client);
        var advisorId = await CreateCareerAdvisorAsync(client, adminAccessToken);

        var (candidateCvId, accessToken) = await RegisterAndLoginCandidateAsync(client);

        var candidateCv = await GetCandidateCvAsync(client, candidateCvId, accessToken);

        Assert.Equal(advisorId, candidateCv.CareerAdvisorId);
    }

    [Fact]
    public async Task Register_TwiceWithTwoAdvisors_AssignsSecondCandidateToTheStillIdleAdvisor()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var adminAccessToken = await LoginAsAdminAsync(factory, client);
        var firstAdvisorId = await CreateCareerAdvisorAsync(client, adminAccessToken);
        var secondAdvisorId = await CreateCareerAdvisorAsync(client, adminAccessToken);

        var (firstCandidateCvId, firstAccessToken) = await RegisterAndLoginCandidateAsync(client);
        var firstCandidateCv = await GetCandidateCvAsync(client, firstCandidateCvId, firstAccessToken);
        var firstAssignedAdvisorId = firstCandidateCv.CareerAdvisorId!.Value;
        var stillIdleAdvisorId = firstAssignedAdvisorId == firstAdvisorId ? secondAdvisorId : firstAdvisorId;

        var (secondCandidateCvId, secondAccessToken) = await RegisterAndLoginCandidateAsync(client);
        var secondCandidateCv = await GetCandidateCvAsync(client, secondCandidateCvId, secondAccessToken);

        Assert.Equal(stillIdleAdvisorId, secondCandidateCv.CareerAdvisorId);
    }
}
