using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Candidate.Features.GetCandidateCv;
using GenclikMerkezi.Modules.Candidate.Features.GetCandidateCvContent;
using GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;
using GenclikMerkezi.Modules.Identity.Features.Login;

namespace GenclikMerkezi.IntegrationTests.Candidate;

public class CandidateCvCrudFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CandidateCvCrudFlowTests(CustomWebApplicationFactory factory)
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

    [Fact]
    public async Task GetCandidateCv_AsOwner_ReturnsSeededData()
    {
        var (candidateCvId, accessToken) = await RegisterAndLoginAsync();

        var request = AuthorizedRequest(HttpMethod.Get, $"/api/v1/candidates/{candidateCvId}", accessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetCandidateCvResponse>();
        Assert.Equal("Ahmet", body!.FirstName);
        Assert.Equal(0, body.CompletionPercentage);
    }

    [Fact]
    public async Task GetCandidateCv_WithoutAuthentication_ReturnsUnauthorized()
    {
        var (candidateCvId, _) = await RegisterAndLoginAsync();

        var response = await _client.GetAsync($"/api/v1/candidates/{candidateCvId}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCandidateCv_AsDifferentCandidate_ReturnsForbidden()
    {
        var (candidateCvId, _) = await RegisterAndLoginAsync();
        var (_, otherAccessToken) = await RegisterAndLoginAsync();

        var request = AuthorizedRequest(HttpMethod.Get, $"/api/v1/candidates/{candidateCvId}", otherAccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateContactInfo_AsOwner_PersistsChangesAndSocialMediaLinks()
    {
        var (candidateCvId, accessToken) = await RegisterAndLoginAsync();

        var updateRequest = AuthorizedRequest(HttpMethod.Put, $"/api/v1/candidates/{candidateCvId}/contact-info", accessToken);
        updateRequest.Content = JsonContent.Create(new
        {
            firstName = "Ahmet",
            lastName = "Yılmaz",
            email = "ahmet.yilmaz@example.com",
            phoneNumber = "05551234567",
            countryId = (Guid?)null,
            provinceId = (Guid?)null,
            districtId = (Guid?)null,
            address = "Kadıköy, İstanbul",
            socialMediaLinks = new[] { new { platform = "GitHub", url = "https://github.com/ahmet" } },
        });
        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getRequest = AuthorizedRequest(HttpMethod.Get, $"/api/v1/candidates/{candidateCvId}", accessToken);
        var getResponse = await _client.SendAsync(getRequest);
        var body = await getResponse.Content.ReadFromJsonAsync<GetCandidateCvResponse>();

        Assert.Equal("Kadıköy, İstanbul", body!.Address);
        Assert.Single(body.SocialMediaLinks);
        Assert.Equal("GitHub", body.SocialMediaLinks[0].Platform);
        Assert.True(body.CompletionPercentage > 0);
    }

    [Fact]
    public async Task UpdatePersonalInfo_WithDisabilityInfo_PersistsDisabilityBlock()
    {
        var (candidateCvId, accessToken) = await RegisterAndLoginAsync();

        var updateRequest = AuthorizedRequest(HttpMethod.Put, $"/api/v1/candidates/{candidateCvId}/personal-info", accessToken);
        updateRequest.Content = JsonContent.Create(new
        {
            title = "Yazılım Uzmanı",
            genderId = (Guid?)null,
            birthDate = (DateOnly?)null,
            driversLicenseTypeId = (Guid?)null,
            nationalityId = (Guid?)null,
            netSalaryExpectation = 45000m,
            militaryStatusId = (Guid?)null,
            disabilityInfo = new
            {
                categoryId = Guid.NewGuid(),
                percentage = 30,
                description = "Açıklama",
                hasHealthReport = true,
                usesMedication = false,
                hasChronicCondition = true,
                hasContagiousDisease = false,
                hasConsciousnessLossRisk = false,
            },
        });
        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getRequest = AuthorizedRequest(HttpMethod.Get, $"/api/v1/candidates/{candidateCvId}", accessToken);
        var getResponse = await _client.SendAsync(getRequest);
        var body = await getResponse.Content.ReadFromJsonAsync<GetCandidateCvResponse>();

        Assert.Equal("Yazılım Uzmanı", body!.Title);
        Assert.NotNull(body.DisabilityInfo);
        Assert.Equal(30, body.DisabilityInfo!.Percentage);
    }

    [Fact]
    public async Task GetCandidateCvContent_AsOwner_ReturnsEmptyContentInitially()
    {
        var (candidateCvId, accessToken) = await RegisterAndLoginAsync();

        var request = AuthorizedRequest(HttpMethod.Get, $"/api/v1/candidates/{candidateCvId}/content", accessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<GetCandidateCvContentResponse>();
        Assert.Equal(candidateCvId, body!.CandidateCvId);
        Assert.Empty(body.Experiences);
    }

    [Fact]
    public async Task UpdateContentSummary_AsOwner_PersistsFields()
    {
        var (candidateCvId, accessToken) = await RegisterAndLoginAsync();

        var updateRequest = AuthorizedRequest(HttpMethod.Put, $"/api/v1/candidates/{candidateCvId}/content/summary", accessToken);
        updateRequest.Content = JsonContent.Create(new
        {
            summary = "Deneyimli yazılım geliştirici.",
            computerSkills = "C#, SQL Server",
            hobbies = "Kitap okumak",
        });
        var updateResponse = await _client.SendAsync(updateRequest);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getRequest = AuthorizedRequest(HttpMethod.Get, $"/api/v1/candidates/{candidateCvId}/content", accessToken);
        var getResponse = await _client.SendAsync(getRequest);
        var body = await getResponse.Content.ReadFromJsonAsync<GetCandidateCvContentResponse>();

        Assert.Equal("Deneyimli yazılım geliştirici.", body!.Summary);
        Assert.Equal("C#, SQL Server", body.ComputerSkills);
    }
}
