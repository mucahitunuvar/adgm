using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;
using GenclikMerkezi.Modules.Candidate.Features.SearchCandidates;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Candidate;

public class SearchCandidatesFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SearchCandidatesFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(Guid CandidateCvId, string AccessToken)> RegisterAndLoginAsync(
        string firstName = "Ahmet", string lastName = "Yılmaz")
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        const string password = "Sifre123";

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/v1/candidates/register",
            new { email, password, firstName, lastName, phoneNumber = (string?)null });
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<RegisterCandidateResponse>();

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        return (registerBody!.CandidateCvId, loginBody!.AccessToken);
    }

    private async Task<string> AdminLoginAsync()
    {
        var adminEmail = $"admin-{Guid.NewGuid():N}@example.com";
        const string adminPassword = "AdminSifre123";
        await _factory.SeedAdminUserAsync(adminEmail, adminPassword);
        var loginResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/login", new { email = adminEmail, password = adminPassword });
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return loginBody!.AccessToken;
    }

    private HttpRequestMessage AuthorizedRequest(HttpMethod method, string url, string accessToken)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private async Task<SearchCandidatesResponse> SearchAsync(string adminAccessToken, string queryString)
    {
        var request = AuthorizedRequest(HttpMethod.Get, $"/api/v1/candidates{queryString}", adminAccessToken);
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<SearchCandidatesResponse>())!;
    }

    private async Task<(Guid Id, string DisplayName)> GetSeededProvinceAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var reader = scope.ServiceProvider.GetRequiredService<IReferenceDataLookupReader>();
        var provinces = await reader.ListAsync(ReferenceDataLookupType.Province, new PagedRequest { PageSize = 1 });
        var province = Assert.Single(provinces.Items);
        return (province.Id, province.DisplayName);
    }

    [Fact]
    public async Task SearchCandidates_AsAdmin_ReturnsRegisteredCandidateBySearchText()
    {
        var (candidateCvId, _) = await RegisterAndLoginAsync("Mehmet", "Demir");
        var adminAccessToken = await AdminLoginAsync();

        var body = await SearchAsync(adminAccessToken, "?searchText=Mehmet");

        Assert.Contains(body.Items, i => i.CandidateCvId == candidateCvId && i.FullName == "Mehmet Demir");
    }

    [Fact]
    public async Task SearchCandidates_AsCandidate_ReturnsForbidden()
    {
        var (_, accessToken) = await RegisterAndLoginAsync();

        var response = await _client.SendAsync(AuthorizedRequest(HttpMethod.Get, "/api/v1/candidates", accessToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SearchCandidates_WithoutAuthentication_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/candidates");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SearchCandidates_FilterByProvinceId_ReturnsOnlyMatchingCandidate_AndResolvesProvinceName()
    {
        var (matchingId, matchingToken) = await RegisterAndLoginAsync("Elif", "Kaya");
        var (otherId, _) = await RegisterAndLoginAsync("Deniz", "Aydın");
        var (provinceId, provinceName) = await GetSeededProvinceAsync();

        var updateRequest = AuthorizedRequest(HttpMethod.Put, $"/api/v1/candidates/{matchingId}/contact-info", matchingToken);
        updateRequest.Content = JsonContent.Create(new
        {
            firstName = "Elif",
            lastName = "Kaya",
            email = "elif.kaya@example.com",
            phoneNumber = (string?)null,
            countryId = (Guid?)null,
            provinceId,
            districtId = (Guid?)null,
            address = (string?)null,
            socialMediaLinks = Array.Empty<object>(),
        });
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(updateRequest)).StatusCode);

        var adminAccessToken = await AdminLoginAsync();
        var body = await SearchAsync(adminAccessToken, $"?provinceId={provinceId}");

        var item = Assert.Single(body.Items, i => i.CandidateCvId == matchingId);
        Assert.Equal(provinceName, item.ProvinceName);
        Assert.DoesNotContain(body.Items, i => i.CandidateCvId == otherId);
    }

    [Fact]
    public async Task SearchCandidates_FilterBySectorId_ReturnsOnlyCandidatesWithThatSector()
    {
        var (matchingId, matchingToken) = await RegisterAndLoginAsync();
        var (otherId, _) = await RegisterAndLoginAsync();
        var sectorId = Guid.NewGuid();

        var addExperienceRequest = AuthorizedRequest(
            HttpMethod.Post, $"/api/v1/candidates/{matchingId}/content/experiences", matchingToken);
        addExperienceRequest.Content = JsonContent.Create(new
        {
            companyName = "Acme A.Ş.",
            positionId = (Guid?)null,
            startDate = new DateOnly(2020, 1, 1),
            endDate = (DateOnly?)null,
            isCurrentJob = true,
            sectorId,
            workFieldId = (Guid?)null,
            employmentTypeId = (Guid?)null,
            countryId = (Guid?)null,
            provinceId = (Guid?)null,
            jobDescription = (string?)null,
        });
        Assert.Equal(HttpStatusCode.Created, (await _client.SendAsync(addExperienceRequest)).StatusCode);

        var adminAccessToken = await AdminLoginAsync();
        var body = await SearchAsync(adminAccessToken, $"?sectorId={sectorId}");

        Assert.Contains(body.Items, i => i.CandidateCvId == matchingId);
        Assert.DoesNotContain(body.Items, i => i.CandidateCvId == otherId);
    }

    [Fact]
    public async Task SearchCandidates_FilterByEducationLevelId_ReturnsOnlyCandidatesWithThatEducationLevel()
    {
        var (matchingId, matchingToken) = await RegisterAndLoginAsync();
        var (otherId, _) = await RegisterAndLoginAsync();
        var educationLevelId = Guid.NewGuid();

        var addEducationRequest = AuthorizedRequest(
            HttpMethod.Post, $"/api/v1/candidates/{matchingId}/content/educations", matchingToken);
        addEducationRequest.Content = JsonContent.Create(new
        {
            educationLevelId,
            startDate = new DateOnly(2016, 9, 1),
            completionStatus = "Continuing",
            endDate = (DateOnly?)null,
            diplomaGradingSystemId = (Guid?)null,
            diplomaGrade = (decimal?)null,
            schoolId = (Guid?)null,
            schoolNameFreeText = (string?)null,
            provinceId = (Guid?)null,
            description = (string?)null,
        });
        Assert.Equal(HttpStatusCode.Created, (await _client.SendAsync(addEducationRequest)).StatusCode);

        var adminAccessToken = await AdminLoginAsync();
        var body = await SearchAsync(adminAccessToken, $"?educationLevelId={educationLevelId}");

        Assert.Contains(body.Items, i => i.CandidateCvId == matchingId);
        Assert.DoesNotContain(body.Items, i => i.CandidateCvId == otherId);
    }

    [Fact]
    public async Task SearchCandidates_FilterByMinCompletionPercentage_ExcludesCandidatesBelowThreshold()
    {
        var (higherId, higherToken) = await RegisterAndLoginAsync();
        var (lowerId, _) = await RegisterAndLoginAsync();

        var updateRequest = AuthorizedRequest(HttpMethod.Put, $"/api/v1/candidates/{higherId}/contact-info", higherToken);
        updateRequest.Content = JsonContent.Create(new
        {
            firstName = "Ahmet",
            lastName = "Yılmaz",
            email = "ahmet.yilmaz@example.com",
            phoneNumber = (string?)null,
            countryId = (Guid?)null,
            provinceId = (Guid?)null,
            districtId = (Guid?)null,
            address = "Kadıköy, İstanbul",
            socialMediaLinks = new[] { new { platform = "GitHub", url = "https://github.com/ahmet" } },
        });
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(updateRequest)).StatusCode);

        var adminAccessToken = await AdminLoginAsync();
        var body = await SearchAsync(adminAccessToken, "?minCompletionPercentage=1");

        Assert.Contains(body.Items, i => i.CandidateCvId == higherId);
        Assert.DoesNotContain(body.Items, i => i.CandidateCvId == lowerId);
    }

    [Fact]
    public async Task SearchCandidates_Pagination_ReturnsRequestedPageSizeAndCorrectMetadata()
    {
        var marker = Guid.NewGuid().ToString("N")[..8];
        for (var i = 0; i < 3; i++)
        {
            await RegisterAndLoginAsync($"Page{marker}{i}", "Test");
        }

        var adminAccessToken = await AdminLoginAsync();
        var body = await SearchAsync(adminAccessToken, $"?searchText=Page{marker}&page=1&pageSize=2");

        Assert.Equal(2, body.Items.Count);
        Assert.Equal(3, body.TotalCount);
        Assert.Equal(1, body.Page);
        Assert.Equal(2, body.PageSize);
        Assert.Equal(2, body.TotalPages);
        Assert.True(body.HasNextPage);
        Assert.False(body.HasPreviousPage);
    }
}
