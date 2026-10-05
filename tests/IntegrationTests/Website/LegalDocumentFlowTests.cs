using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.CreateLegalDocument;
using GenclikMerkezi.Modules.Website.Features.CreateLegalDocumentDraft;
using GenclikMerkezi.Modules.Website.Features.DeleteLegalDocumentDraft;
using GenclikMerkezi.Modules.Website.Features.GetLegalDocumentById;
using GenclikMerkezi.Modules.Website.Features.GetLegalDocuments;
using GenclikMerkezi.Modules.Website.Features.GetPublicLegalDocument;
using GenclikMerkezi.Modules.Website.Features.GetPublicLegalDocumentVersion;
using GenclikMerkezi.Modules.Website.Features.PublishLegalDocumentDraft;
using GenclikMerkezi.Modules.Website.Features.UpdateLegalDocumentDraftBody;
using GenclikMerkezi.Modules.Website.Features.UpdateLegalDocumentTranslation;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §12.1 (Faz 3 Görev 2). Shares the "one CustomWebApplicationFactory/one Sqlite database per
// class" caveat every other Website flow test class documents.
public class LegalDocumentFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LegalDocumentFlowTests(CustomWebApplicationFactory factory)
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

    private HttpRequestMessage Authorized(HttpMethod method, string path, string accessToken, object? body = null)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }

    private async Task<CreateLegalDocumentResponse> CreateDocumentAsync(
        string accessToken, string? key = null, string kind = "PrivacyNotice", string title = "KVKK Aydınlatma Metni")
    {
        key ??= $"kvkk-{Guid.NewGuid():N}";
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/legal-documents", accessToken,
            new CreateLegalDocumentRequest(key, kind, title)));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CreateLegalDocumentResponse>())!;
    }

    private async Task<LegalDocumentDetailResponse> GetDocumentAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/legal-documents/{id}", accessToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<LegalDocumentDetailResponse>())!;
    }

    private async Task<CreateLegalDocumentDraftResponse> CreateDraftAsync(string accessToken, Guid id, byte[] rowVersion, string? changeSummary = null)
    {
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/legal-documents/{id}/versions/draft", accessToken,
            new CreateLegalDocumentDraftRequest(rowVersion, changeSummary)));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CreateLegalDocumentDraftResponse>())!;
    }

    private async Task UpdateDraftBodyAsync(string accessToken, Guid id, string lang, byte[] rowVersion, string body)
    {
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/legal-documents/{id}/versions/draft/translations/{lang}", accessToken,
            new UpdateLegalDocumentDraftBodyRequest(rowVersion, body)));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private async Task<HttpResponseMessage> PublishDraftAsync(string accessToken, Guid id, byte[] rowVersion, DateTime? effectiveAtUtc = null) =>
        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/legal-documents/{id}/versions/draft/publish", accessToken,
            new PublishLegalDocumentDraftRequest(rowVersion, effectiveAtUtc)));

    [Fact]
    public async Task CreateLegalDocument_WithValidInput_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();

        var created = await CreateDocumentAsync(accessToken);
        var detail = await GetDocumentAsync(accessToken, created.Id);

        Assert.Equal("PrivacyNotice", detail.Kind);
        Assert.Single(detail.Translations, t => t.LanguageCode == "tr" && t.Title == "KVKK Aydınlatma Metni");
        Assert.Empty(detail.Versions);
        Assert.Null(detail.EffectiveVersionNumber);
    }

    [Fact]
    public async Task CreateLegalDocument_WithDuplicateKey_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = $"kvkk-{Guid.NewGuid():N}";
        await CreateDocumentAsync(accessToken, key: key);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/legal-documents", accessToken,
            new CreateLegalDocumentRequest(key, "PrivacyNotice", "Başka Başlık")));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateLegalDocument_WithUnrecognizedKind_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/legal-documents", accessToken,
            new CreateLegalDocumentRequest($"kvkk-{Guid.NewGuid():N}", "NotAKind", "Title")));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateLegalDocumentTranslation_WithStaleRowVersion_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateDocumentAsync(accessToken);
        var staleRowVersion = (await GetDocumentAsync(accessToken, created.Id)).RowVersion;

        var first = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/legal-documents/{created.Id}/translations/en", accessToken,
            new UpdateLegalDocumentTranslationRequest(staleRowVersion, "Privacy Notice")));
        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);

        var second = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/legal-documents/{created.Id}/translations/en", accessToken,
            new UpdateLegalDocumentTranslationRequest(staleRowVersion, "Privacy Notice v2")));

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task PublishDraft_WithoutDefaultLanguageBody_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateDocumentAsync(accessToken);
        var rowVersion = (await GetDocumentAsync(accessToken, created.Id)).RowVersion;
        await CreateDraftAsync(accessToken, created.Id, rowVersion);
        rowVersion = (await GetDocumentAsync(accessToken, created.Id)).RowVersion;

        var response = await PublishDraftAsync(accessToken, created.Id, rowVersion);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task FullLifecycle_CreateDraftPublish_IsVisibleOnThePublicEndpoints()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = $"kvkk-{Guid.NewGuid():N}";
        var created = await CreateDocumentAsync(accessToken, key: key);
        var rowVersion = (await GetDocumentAsync(accessToken, created.Id)).RowVersion;

        await CreateDraftAsync(accessToken, created.Id, rowVersion);
        rowVersion = (await GetDocumentAsync(accessToken, created.Id)).RowVersion;
        await UpdateDraftBodyAsync(accessToken, created.Id, "tr", rowVersion, "<p>Kişisel verileriniz işlenir.</p>");
        rowVersion = (await GetDocumentAsync(accessToken, created.Id)).RowVersion;

        var publishResponse = await PublishDraftAsync(accessToken, created.Id, rowVersion);
        Assert.Equal(HttpStatusCode.NoContent, publishResponse.StatusCode);

        var detail = await GetDocumentAsync(accessToken, created.Id);
        Assert.Equal(1, detail.EffectiveVersionNumber);
        Assert.False(detail.Versions.Single().Translations.Count == 0);

        var publicResponse = await _client.GetAsync($"/api/v1/public/legal-documents/{key}?lang=tr");
        Assert.Equal(HttpStatusCode.OK, publicResponse.StatusCode);
        var publicDocument = await publicResponse.Content.ReadFromJsonAsync<PublicLegalDocumentResponse>();
        Assert.Equal(1, publicDocument!.VersionNumber);
        Assert.Equal("<p>Kişisel verileriniz işlenir.</p>", publicDocument.Body);

        var publicVersionResponse = await _client.GetAsync($"/api/v1/public/legal-documents/{key}/versions/1?lang=tr");
        Assert.Equal(HttpStatusCode.OK, publicVersionResponse.StatusCode);
        var publicVersion = await publicVersionResponse.Content.ReadFromJsonAsync<PublicLegalDocumentVersionResponse>();
        Assert.Equal("<p>Kişisel verileriniz işlenir.</p>", publicVersion!.Body);
    }

    [Fact]
    public async Task PublicLegalDocument_WithoutAnyEffectiveVersion_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = $"kvkk-{Guid.NewGuid():N}";
        await CreateDocumentAsync(accessToken, key: key);

        var response = await _client.GetAsync($"/api/v1/public/legal-documents/{key}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PublicLegalDocument_WithUnknownKey_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/v1/public/legal-documents/unknown-{Guid.NewGuid():N}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PublicLegalDocumentVersion_ForADraftVersionNumber_ReturnsNotFound()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = $"kvkk-{Guid.NewGuid():N}";
        var created = await CreateDocumentAsync(accessToken, key: key);
        var rowVersion = (await GetDocumentAsync(accessToken, created.Id)).RowVersion;
        await CreateDraftAsync(accessToken, created.Id, rowVersion);

        var response = await _client.GetAsync($"/api/v1/public/legal-documents/{key}/versions/1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteLegalDocumentDraft_RemovesTheDraft()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateDocumentAsync(accessToken);
        var rowVersion = (await GetDocumentAsync(accessToken, created.Id)).RowVersion;
        await CreateDraftAsync(accessToken, created.Id, rowVersion);
        rowVersion = (await GetDocumentAsync(accessToken, created.Id)).RowVersion;

        var deleteResponse = await _client.SendAsync(Authorized(
            HttpMethod.Delete, $"/api/v1/admin/website/legal-documents/{created.Id}/versions/draft", accessToken,
            new DeleteLegalDocumentDraftRequest(rowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var detail = await GetDocumentAsync(accessToken, created.Id);
        Assert.Empty(detail.Versions);
    }

    [Fact]
    public async Task DeleteLegalDocument_WhenNeverPublished_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateDocumentAsync(accessToken);

        var deleteResponse = await _client.SendAsync(
            Authorized(HttpMethod.Delete, $"/api/v1/admin/website/legal-documents/{created.Id}", accessToken));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDeleteResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/legal-documents/{created.Id}", accessToken));
        Assert.Equal(HttpStatusCode.NotFound, afterDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteLegalDocument_AfterEverBeingPublished_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateDocumentAsync(accessToken);
        var rowVersion = (await GetDocumentAsync(accessToken, created.Id)).RowVersion;
        await CreateDraftAsync(accessToken, created.Id, rowVersion);
        rowVersion = (await GetDocumentAsync(accessToken, created.Id)).RowVersion;
        await UpdateDraftBodyAsync(accessToken, created.Id, "tr", rowVersion, "<p>Gövde</p>");
        rowVersion = (await GetDocumentAsync(accessToken, created.Id)).RowVersion;
        await PublishDraftAsync(accessToken, created.Id, rowVersion);

        var deleteResponse = await _client.SendAsync(
            Authorized(HttpMethod.Delete, $"/api/v1/admin/website/legal-documents/{created.Id}", accessToken));

        Assert.Equal(HttpStatusCode.Conflict, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task GetLegalDocuments_ListsCreatedDocumentWithDraftAndEffectiveVersionState()
    {
        var accessToken = await LoginAsAdminAsync();
        var created = await CreateDocumentAsync(accessToken);
        var rowVersion = (await GetDocumentAsync(accessToken, created.Id)).RowVersion;
        await CreateDraftAsync(accessToken, created.Id, rowVersion);

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/legal-documents", accessToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<IReadOnlyList<LegalDocumentSummaryResponse>>();

        var summary = Assert.Single(list!, d => d.Id == created.Id);
        Assert.True(summary.HasDraft);
        Assert.Null(summary.EffectiveVersionNumber);
    }
}
