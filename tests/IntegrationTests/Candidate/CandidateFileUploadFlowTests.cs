using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Candidate.Features.GetCandidateCv;
using GenclikMerkezi.Modules.Candidate.Features.GetCandidateCvContent;
using GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;
using GenclikMerkezi.Modules.Identity.Features.Login;

namespace GenclikMerkezi.IntegrationTests.Candidate;

public class CandidateFileUploadFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CandidateFileUploadFlowTests(CustomWebApplicationFactory factory)
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

    private static MultipartFormDataContent BuildFileContent(byte[] bytes, string fileName, string contentType)
    {
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);
        return content;
    }

    [Fact]
    public async Task UploadPhoto_WithValidJpeg_SetsPhotoUrl()
    {
        var (candidateCvId, accessToken) = await RegisterAndLoginAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/candidates/{candidateCvId}/photo")
        {
            Content = BuildFileContent([1, 2, 3, 4], "photo.jpg", "image/jpeg"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/candidates/{candidateCvId}");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var getResponse = await _client.SendAsync(getRequest);
        var body = await getResponse.Content.ReadFromJsonAsync<GetCandidateCvResponse>();

        Assert.NotNull(body!.PhotoUrl);
        Assert.True(body.CompletionPercentage > 0);
    }

    [Fact]
    public async Task UploadPhoto_WithDisallowedExtension_ReturnsBadRequest()
    {
        var (candidateCvId, accessToken) = await RegisterAndLoginAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/candidates/{candidateCvId}/photo")
        {
            Content = BuildFileContent([1, 2, 3, 4], "cv.pdf", "image/jpeg"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadCvFile_WithValidPdf_SetsCvFileUrl()
    {
        var (candidateCvId, accessToken) = await RegisterAndLoginAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/candidates/{candidateCvId}/content/cv-file")
        {
            Content = BuildFileContent([1, 2, 3, 4], "cv.pdf", "application/pdf"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/candidates/{candidateCvId}/content");
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var getResponse = await _client.SendAsync(getRequest);
        var body = await getResponse.Content.ReadFromJsonAsync<GetCandidateCvContentResponse>();

        Assert.NotNull(body!.CvFileUrl);
    }
}
