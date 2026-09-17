using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Candidate.Features.GetCandidateCv;
using GenclikMerkezi.Modules.Candidate.Features.GetCandidateCvContent;
using GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;
using GenclikMerkezi.Modules.Identity.Features.Login;

namespace GenclikMerkezi.IntegrationTests.Candidate;

// Exercises the full Add -> Update -> Remove lifecycle for one of the five CandidateCvContent
// collections (Experience) end to end, as the representative case for the identical pattern the
// other four (Education/CandidateLanguage/Certificate/CandidateReference) share.
public class ExperienceCrudFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExperienceCrudFlowTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<(Guid CandidateCvId, string AccessToken)> RegisterAndLoginAsync()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        const string password = "Sifre123";

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/v1/candidates/register",
            new { email, password, firstName = "Ahmet", lastName = "Yılmaz", phoneNumber = (string?)null });
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<RegisterCandidateResponse>();

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        return (registerBody!.CandidateCvId, loginBody!.AccessToken);
    }

    private HttpRequestMessage AuthorizedRequest(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private async Task<GetCandidateCvResponse> GetCandidateCvAsync(Guid candidateCvId, string accessToken)
    {
        var request = AuthorizedRequest(HttpMethod.Get, $"/api/v1/candidates/{candidateCvId}", accessToken);
        var response = await _client.SendAsync(request);
        return (await response.Content.ReadFromJsonAsync<GetCandidateCvResponse>())!;
    }

    [Fact]
    public async Task AddExperience_ThenUpdateThenRemove_FullLifecycle()
    {
        var (candidateCvId, accessToken) = await RegisterAndLoginAsync();

        var addRequest = AuthorizedRequest(HttpMethod.Post, $"/api/v1/candidates/{candidateCvId}/content/experiences", accessToken);
        addRequest.Content = JsonContent.Create(new
        {
            companyName = "Acme A.Ş.",
            positionId = (Guid?)null,
            startDate = new DateOnly(2020, 1, 1),
            endDate = (DateOnly?)null,
            isCurrentJob = true,
            sectorId = (Guid?)null,
            workFieldId = (Guid?)null,
            employmentTypeId = (Guid?)null,
            countryId = (Guid?)null,
            provinceId = (Guid?)null,
            jobDescription = "Backend geliştirme.",
        });
        var addResponse = await _client.SendAsync(addRequest);
        Assert.Equal(HttpStatusCode.Created, addResponse.StatusCode);
        var created = await addResponse.Content.ReadFromJsonAsync<Dictionary<string, Guid>>();
        var experienceId = created!["id"];

        // Adding an Experience is one of the eight completion criteria - the count going 0 -> 1
        // should raise CandidateCvContentUpdatedDomainEvent and recalculate CandidateCv's percentage.
        var afterAdd = await GetCandidateCvAsync(candidateCvId, accessToken);
        Assert.True(afterAdd.CompletionPercentage > 0);

        var updateRequest = AuthorizedRequest(
            HttpMethod.Put, $"/api/v1/candidates/{candidateCvId}/content/experiences/{experienceId}", accessToken);
        updateRequest.Content = JsonContent.Create(new
        {
            companyName = "Yeni Şirket A.Ş.",
            positionId = (Guid?)null,
            startDate = new DateOnly(2020, 1, 1),
            endDate = new DateOnly(2023, 6, 1),
            isCurrentJob = false,
            sectorId = (Guid?)null,
            workFieldId = (Guid?)null,
            employmentTypeId = (Guid?)null,
            countryId = (Guid?)null,
            provinceId = (Guid?)null,
            jobDescription = "Güncellenmiş iş tanımı.",
        });
        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var contentRequest = AuthorizedRequest(HttpMethod.Get, $"/api/v1/candidates/{candidateCvId}/content", accessToken);
        var contentResponse = await _client.SendAsync(contentRequest);
        var content = await contentResponse.Content.ReadFromJsonAsync<GetCandidateCvContentResponse>();
        var experience = Assert.Single(content!.Experiences);
        Assert.Equal("Yeni Şirket A.Ş.", experience.CompanyName);
        Assert.False(experience.IsCurrentJob);

        var removeRequest = AuthorizedRequest(
            HttpMethod.Delete, $"/api/v1/candidates/{candidateCvId}/content/experiences/{experienceId}", accessToken);
        var removeResponse = await _client.SendAsync(removeRequest);
        Assert.Equal(HttpStatusCode.NoContent, removeResponse.StatusCode);

        var afterRemove = await GetCandidateCvAsync(candidateCvId, accessToken);
        Assert.Equal(0, afterRemove.CompletionPercentage);
    }

    [Fact]
    public async Task AddExperience_AsDifferentCandidate_ReturnsForbidden()
    {
        var (candidateCvId, _) = await RegisterAndLoginAsync();
        var (_, otherAccessToken) = await RegisterAndLoginAsync();

        var addRequest = AuthorizedRequest(HttpMethod.Post, $"/api/v1/candidates/{candidateCvId}/content/experiences", otherAccessToken);
        addRequest.Content = JsonContent.Create(new
        {
            companyName = "Acme A.Ş.",
            positionId = (Guid?)null,
            startDate = new DateOnly(2020, 1, 1),
            endDate = (DateOnly?)null,
            isCurrentJob = true,
            sectorId = (Guid?)null,
            workFieldId = (Guid?)null,
            employmentTypeId = (Guid?)null,
            countryId = (Guid?)null,
            provinceId = (Guid?)null,
            jobDescription = (string?)null,
        });
        var response = await _client.SendAsync(addRequest);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
