using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;
using GenclikMerkezi.Modules.Identity.Features.Login;
using UglyToad.PdfPig;

namespace GenclikMerkezi.IntegrationTests.Candidate;

public class ExportCandidateCvPdfFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ExportCandidateCvPdfFlowTests(CustomWebApplicationFactory factory)
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

    private static string ExtractText(byte[] pdfBytes)
    {
        using var document = PdfDocument.Open(pdfBytes);
        return string.Join(" ", document.GetPages().Select(p => p.Text));
    }

    [Fact]
    public async Task ExportCandidateCvPdf_AsOwner_ReturnsPdfWithFilledSections()
    {
        var (candidateCvId, accessToken) = await RegisterAndLoginAsync("Elif", "Kaya");

        var updateRequest = AuthorizedRequest(HttpMethod.Put, $"/api/v1/candidates/{candidateCvId}/contact-info", accessToken);
        updateRequest.Content = JsonContent.Create(new
        {
            firstName = "Elif",
            lastName = "Kaya",
            email = "elif.kaya@example.com",
            phoneNumber = "05551112233",
            countryId = (Guid?)null,
            provinceId = (Guid?)null,
            districtId = (Guid?)null,
            address = "Beşiktaş, İstanbul",
            socialMediaLinks = new[] { new { platform = "GitHub", url = "https://github.com/elif" } },
        });
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(updateRequest)).StatusCode);

        var summaryRequest = AuthorizedRequest(HttpMethod.Put, $"/api/v1/candidates/{candidateCvId}/content/summary", accessToken);
        summaryRequest.Content = JsonContent.Create(new
        {
            summary = "Deneyimli backend geliştirici.",
            computerSkills = "C#, SQL Server",
            hobbies = "Satranç",
        });
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(summaryRequest)).StatusCode);

        var experienceRequest = AuthorizedRequest(
            HttpMethod.Post, $"/api/v1/candidates/{candidateCvId}/content/experiences", accessToken);
        experienceRequest.Content = JsonContent.Create(new
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
        Assert.Equal(HttpStatusCode.Created, (await _client.SendAsync(experienceRequest)).StatusCode);

        var exportRequest = AuthorizedRequest(HttpMethod.Get, $"/api/v1/candidates/{candidateCvId}/cv/export", accessToken);
        var response = await _client.SendAsync(exportRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("Elif_Kaya_CV.pdf", response.Content.Headers.ContentDisposition?.FileName);

        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
        Assert.NotEmpty(pdfBytes);

        var text = ExtractText(pdfBytes);
        Assert.Contains("Elif Kaya", text);
        Assert.Contains("Deneyimli backend geliştirici.", text);
        Assert.Contains("Acme A.Ş.", text);
        Assert.Contains("Satranç", text);
    }

    [Fact]
    public async Task ExportCandidateCvPdf_AsDifferentCandidate_ReturnsForbidden()
    {
        var (candidateCvId, _) = await RegisterAndLoginAsync();
        var (_, otherAccessToken) = await RegisterAndLoginAsync();

        var request = AuthorizedRequest(HttpMethod.Get, $"/api/v1/candidates/{candidateCvId}/cv/export", otherAccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ExportCandidateCvPdf_WithoutAuthentication_ReturnsUnauthorized()
    {
        var (candidateCvId, _) = await RegisterAndLoginAsync();

        var response = await _client.GetAsync($"/api/v1/candidates/{candidateCvId}/cv/export");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ExportCandidateCvPdf_AsAdmin_CanExportAnyCandidatesCv()
    {
        var (candidateCvId, _) = await RegisterAndLoginAsync("Deniz", "Aydın");
        var adminAccessToken = await AdminLoginAsync();

        var request = AuthorizedRequest(HttpMethod.Get, $"/api/v1/candidates/{candidateCvId}/cv/export", adminAccessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var text = ExtractText(await response.Content.ReadAsByteArrayAsync());
        Assert.Contains("Deniz Aydın", text);
    }
}
