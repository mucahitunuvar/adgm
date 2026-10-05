using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.ActivateFormDefinition;
using GenclikMerkezi.Modules.Website.Features.CreateContentItem;
using GenclikMerkezi.Modules.Website.Features.CreateContentType;
using GenclikMerkezi.Modules.Website.Features.CreateFormDefinition;
using GenclikMerkezi.Modules.Website.Features.CreateLegalDocument;
using GenclikMerkezi.Modules.Website.Features.CreateLegalDocumentDraft;
using GenclikMerkezi.Modules.Website.Features.DeactivateFormDefinition;
using GenclikMerkezi.Modules.Website.Features.GetContentItemById;
using GenclikMerkezi.Modules.Website.Features.GetFormDefinitionById;
using GenclikMerkezi.Modules.Website.Features.GetFormDefinitions;
using GenclikMerkezi.Modules.Website.Features.GetLegalDocumentById;
using GenclikMerkezi.Modules.Website.Features.GetPublicContentById;
using GenclikMerkezi.Modules.Website.Features.PublishContentItem;
using GenclikMerkezi.Modules.Website.Features.PublishLegalDocumentDraft;
using GenclikMerkezi.Modules.Website.Features.SetContentItemForm;
using GenclikMerkezi.Modules.Website.Features.SetFormFields;
using GenclikMerkezi.Modules.Website.Features.UpdateFormDefinitionTranslation;
using GenclikMerkezi.Modules.Website.Features.UpdateLegalDocumentDraftBody;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §12.2 (Faz 3 Görev 3). Shares the "one CustomWebApplicationFactory/one Sqlite database per
// class" caveat every other Website flow test class documents.
public class FormDefinitionFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public FormDefinitionFlowTests(CustomWebApplicationFactory factory)
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

    // Goes through Create -> CreateDraft -> UpdateDraftBody -> PublishDraft so the returned key has an
    // effective version, the way FormDefinition.Activate requires (ADR-024 §12.2).
    private async Task<string> CreatePublishedLegalDocumentAsync(string accessToken, string kind)
    {
        var key = $"legal-{Guid.NewGuid():N}";
        var createResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/legal-documents", accessToken,
            new CreateLegalDocumentRequest(key, kind, "Başlık")));
        var created = (await createResponse.Content.ReadFromJsonAsync<CreateLegalDocumentResponse>())!;

        var detailResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/legal-documents/{created.Id}", accessToken));
        var detail = (await detailResponse.Content.ReadFromJsonAsync<LegalDocumentDetailResponse>())!;

        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/legal-documents/{created.Id}/versions/draft", accessToken,
            new CreateLegalDocumentDraftRequest(detail.RowVersion, null)));

        detailResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/legal-documents/{created.Id}", accessToken));
        detail = (await detailResponse.Content.ReadFromJsonAsync<LegalDocumentDetailResponse>())!;

        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/legal-documents/{created.Id}/versions/draft/translations/tr", accessToken,
            new UpdateLegalDocumentDraftBodyRequest(detail.RowVersion, "<p>Gövde</p>")));

        detailResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/legal-documents/{created.Id}", accessToken));
        detail = (await detailResponse.Content.ReadFromJsonAsync<LegalDocumentDetailResponse>())!;

        var publishResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/legal-documents/{created.Id}/versions/draft/publish", accessToken,
            new PublishLegalDocumentDraftRequest(detail.RowVersion, null)));
        Assert.Equal(HttpStatusCode.NoContent, publishResponse.StatusCode);

        return key;
    }

    private async Task<CreateFormDefinitionResponse> CreateFormAsync(
        string accessToken, string privacyNoticeKey, string? key = null,
        IReadOnlyList<CreateFormDefinitionExplicitConsentInput>? explicitConsents = null)
    {
        key ??= $"form-{Guid.NewGuid():N}";
        var request = new CreateFormDefinitionRequest(
            key, null, [], privacyNoticeKey, explicitConsents ?? [], "İletişim Formu", "Açıklama", "Teşekkürler", "Gönder");
        var response = await _client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/admin/website/forms", accessToken, request));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CreateFormDefinitionResponse>())!;
    }

    private async Task<FormDefinitionDetailResponse> GetFormAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/forms/{id}", accessToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<FormDefinitionDetailResponse>())!;
    }

    private static FormFieldInput TextFieldInput(string key) =>
        new(key, "Text", true, 0, null, null, [], null, null, [], null, [new FormFieldTranslationInput("tr", key, null, null)]);

    private static FormFieldInput FileFieldInput(string key) =>
        new(key, "File", false, 0, null, null, [], null, null, ["Pdf"], 5, [new FormFieldTranslationInput("tr", key, null, null)]);

    [Fact]
    public async Task CreateFormDefinition_WithValidInput_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var privacyNoticeKey = await CreatePublishedLegalDocumentAsync(accessToken, "PrivacyNotice");

        var created = await CreateFormAsync(accessToken, privacyNoticeKey);
        var detail = await GetFormAsync(accessToken, created.Id);

        Assert.False(detail.IsActive);
        Assert.Equal(730, detail.RetentionDays);
        Assert.Equal(privacyNoticeKey, detail.PrivacyNoticeKey);
        Assert.Single(detail.Translations, t => t.LanguageCode == "tr" && t.Title == "İletişim Formu");
    }

    [Fact]
    public async Task CreateFormDefinition_WithDuplicateKey_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var privacyNoticeKey = await CreatePublishedLegalDocumentAsync(accessToken, "PrivacyNotice");
        var key = $"form-{Guid.NewGuid():N}";
        await CreateFormAsync(accessToken, privacyNoticeKey, key: key);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/forms", accessToken,
            new CreateFormDefinitionRequest(key, null, [], privacyNoticeKey, [], "Başka", "Açıklama", "Teşekkürler", "Gönder")));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateFormDefinition_WithPrivacyNoticeOfWrongKind_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var cookiePolicyKey = await CreatePublishedLegalDocumentAsync(accessToken, "CookiePolicy");

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/forms", accessToken,
            new CreateFormDefinitionRequest(
                $"form-{Guid.NewGuid():N}", null, [], cookiePolicyKey, [], "Başlık", "Açıklama", "Teşekkürler", "Gönder")));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateFormDefinitionTranslation_WithStaleRowVersion_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var privacyNoticeKey = await CreatePublishedLegalDocumentAsync(accessToken, "PrivacyNotice");
        var created = await CreateFormAsync(accessToken, privacyNoticeKey);
        var staleRowVersion = (await GetFormAsync(accessToken, created.Id)).RowVersion;

        var first = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/forms/{created.Id}/translations/en", accessToken,
            new UpdateFormDefinitionTranslationRequest(staleRowVersion, "Contact Form", "Description", "Thanks", "Submit")));
        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);

        var second = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/forms/{created.Id}/translations/en", accessToken,
            new UpdateFormDefinitionTranslationRequest(staleRowVersion, "Contact Form v2", "Description", "Thanks", "Submit")));

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task SetFormFields_WithValidFields_SucceedsAndIncrementsDefinitionVersion()
    {
        var accessToken = await LoginAsAdminAsync();
        var privacyNoticeKey = await CreatePublishedLegalDocumentAsync(accessToken, "PrivacyNotice");
        var created = await CreateFormAsync(accessToken, privacyNoticeKey);
        var rowVersion = (await GetFormAsync(accessToken, created.Id)).RowVersion;

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/forms/{created.Id}/fields", accessToken,
            new SetFormFieldsRequest(rowVersion, [TextFieldInput("full_name")])));
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var detail = await GetFormAsync(accessToken, created.Id);
        Assert.Equal(1, detail.DefinitionVersion);
        Assert.Single(detail.Fields);
    }

    [Fact]
    public async Task SetFormFields_WithDuplicateKeys_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var privacyNoticeKey = await CreatePublishedLegalDocumentAsync(accessToken, "PrivacyNotice");
        var created = await CreateFormAsync(accessToken, privacyNoticeKey);
        var rowVersion = (await GetFormAsync(accessToken, created.Id)).RowVersion;

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/forms/{created.Id}/fields", accessToken,
            new SetFormFieldsRequest(rowVersion, [TextFieldInput("full_name"), TextFieldInput("full_name")])));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetFormFields_ExceedingMaxFileFields_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var privacyNoticeKey = await CreatePublishedLegalDocumentAsync(accessToken, "PrivacyNotice");
        var created = await CreateFormAsync(accessToken, privacyNoticeKey);
        var rowVersion = (await GetFormAsync(accessToken, created.Id)).RowVersion;

        var fileFields = Enumerable.Range(0, 4).Select(i => FileFieldInput($"file_{i}")).ToList();
        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/forms/{created.Id}/fields", accessToken,
            new SetFormFieldsRequest(rowVersion, fileFields)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ActivateFormDefinition_WithoutEffectivePrivacyNotice_ReturnsConflict()
    {
        var accessToken = await LoginAsAdminAsync();
        var key = $"legal-{Guid.NewGuid():N}";
        await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/legal-documents", accessToken,
            new CreateLegalDocumentRequest(key, "PrivacyNotice", "Başlık")));
        var created = await CreateFormAsync(accessToken, key);
        var rowVersion = (await GetFormAsync(accessToken, created.Id)).RowVersion;

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/forms/{created.Id}/activate", accessToken,
            new ActivateFormDefinitionRequest(rowVersion)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ActivateFormDefinition_WithEffectivePrivacyNotice_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var privacyNoticeKey = await CreatePublishedLegalDocumentAsync(accessToken, "PrivacyNotice");
        var created = await CreateFormAsync(accessToken, privacyNoticeKey);
        var rowVersion = (await GetFormAsync(accessToken, created.Id)).RowVersion;

        var activateResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/forms/{created.Id}/activate", accessToken,
            new ActivateFormDefinitionRequest(rowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, activateResponse.StatusCode);

        var detail = await GetFormAsync(accessToken, created.Id);
        Assert.True(detail.IsActive);

        var deactivateResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/forms/{created.Id}/deactivate", accessToken,
            new DeactivateFormDefinitionRequest(detail.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, deactivateResponse.StatusCode);

        detail = await GetFormAsync(accessToken, created.Id);
        Assert.False(detail.IsActive);
    }

    [Fact]
    public async Task GetFormDefinitions_ListsCreatedForm()
    {
        var accessToken = await LoginAsAdminAsync();
        var privacyNoticeKey = await CreatePublishedLegalDocumentAsync(accessToken, "PrivacyNotice");
        var created = await CreateFormAsync(accessToken, privacyNoticeKey);

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/forms", accessToken));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<FormDefinitionSummaryResponse>>();

        var summary = Assert.Single(list!, f => f.Id == created.Id);
        Assert.False(summary.IsActive);
        Assert.Equal("İletişim Formu", summary.Title);
    }

    [Fact]
    public async Task DeleteFormDefinition_WhenNotLinked_Succeeds()
    {
        var accessToken = await LoginAsAdminAsync();
        var privacyNoticeKey = await CreatePublishedLegalDocumentAsync(accessToken, "PrivacyNotice");
        var created = await CreateFormAsync(accessToken, privacyNoticeKey);

        var deleteResponse = await _client.SendAsync(
            Authorized(HttpMethod.Delete, $"/api/v1/admin/website/forms/{created.Id}", accessToken));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var afterDeleteResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/forms/{created.Id}", accessToken));
        Assert.Equal(HttpStatusCode.NotFound, afterDeleteResponse.StatusCode);
    }

    [Fact]
    public async Task SetContentItemForm_OnTypeNotSupportingForm_ReturnsBadRequest()
    {
        var accessToken = await LoginAsAdminAsync();
        var privacyNoticeKey = await CreatePublishedLegalDocumentAsync(accessToken, "PrivacyNotice");
        var form = await CreateFormAsync(accessToken, privacyNoticeKey);

        var emptySeo = new CreateContentTypeSeoInput(null, null, null, null, null, null, null, false);
        var typeKey = $"type-{Guid.NewGuid():N}"[..20];
        var typeResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/content-types", accessToken,
            new CreateContentTypeRequest(
                typeKey, "list", "detail", "Manual", 1,
                SupportsHierarchy: false, SupportsCategories: false, SupportsTags: false, SupportsDetailImage: false, SupportsGallery: false,
                SupportsVideos: false, SupportsAttachments: false, SupportsEvent: false, SupportsBlockLayout: false, SupportsForm: false,
                SupportsRelatedContent: false, HasDetailPage: false, HasListingPage: false, IsSearchable: false, RequiresReview: false,
                DefaultLanguageName: "Tür", DefaultLanguageRoutePrefix: null, Seo: emptySeo)));
        var type = (await typeResponse.Content.ReadFromJsonAsync<CreateContentTypeResponse>())!;

        var itemSeo = new CreateContentItemSeoInput(null, null, null, null, null, null, null, false);
        var itemResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/contents", accessToken,
            new CreateContentItemRequest(type.Id, null, 1, false, null, null, "Başlık", $"slug-{Guid.NewGuid():N}", null, null, itemSeo)));
        var item = (await itemResponse.Content.ReadFromJsonAsync<CreateContentItemResponse>())!;

        var itemDetailResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{item.Id}", accessToken));
        var itemDetail = (await itemDetailResponse.Content.ReadFromJsonAsync<ContentItemDetailResponse>())!;

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{item.Id}/form", accessToken,
            new SetContentItemFormRequest(itemDetail.RowVersion, form.Id)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SetContentItemForm_OnActiveLinkedForm_AppearsInPublicContentDetail()
    {
        var accessToken = await LoginAsAdminAsync();
        var privacyNoticeKey = await CreatePublishedLegalDocumentAsync(accessToken, "PrivacyNotice");
        var form = await CreateFormAsync(accessToken, privacyNoticeKey);
        var formRowVersion = (await GetFormAsync(accessToken, form.Id)).RowVersion;
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/forms/{form.Id}/fields", accessToken,
            new SetFormFieldsRequest(formRowVersion, [TextFieldInput("full_name")])));
        formRowVersion = (await GetFormAsync(accessToken, form.Id)).RowVersion;
        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/forms/{form.Id}/activate", accessToken,
            new ActivateFormDefinitionRequest(formRowVersion)));

        var emptyTypeSeo = new CreateContentTypeSeoInput(null, null, null, null, null, null, null, false);
        var typeKey = $"type-{Guid.NewGuid():N}"[..20];
        var typeResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/content-types", accessToken,
            new CreateContentTypeRequest(
                typeKey, "list", "detail", "Manual", 1,
                SupportsHierarchy: false, SupportsCategories: false, SupportsTags: false, SupportsDetailImage: false, SupportsGallery: false,
                SupportsVideos: false, SupportsAttachments: false, SupportsEvent: false, SupportsBlockLayout: false, SupportsForm: true,
                SupportsRelatedContent: false, HasDetailPage: true, HasListingPage: false, IsSearchable: false, RequiresReview: false,
                DefaultLanguageName: "Tür", DefaultLanguageRoutePrefix: null, Seo: emptyTypeSeo)));
        var type = (await typeResponse.Content.ReadFromJsonAsync<CreateContentTypeResponse>())!;

        var itemSeo = new CreateContentItemSeoInput(null, null, null, null, null, null, null, false);
        var itemResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/contents", accessToken,
            new CreateContentItemRequest(type.Id, null, 1, false, null, null, "Gönüllülük", $"slug-{Guid.NewGuid():N}", null, null, itemSeo)));
        var item = (await itemResponse.Content.ReadFromJsonAsync<CreateContentItemResponse>())!;

        var itemDetailResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{item.Id}", accessToken));
        var itemDetail = (await itemDetailResponse.Content.ReadFromJsonAsync<ContentItemDetailResponse>())!;

        var linkResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/contents/{item.Id}/form", accessToken,
            new SetContentItemFormRequest(itemDetail.RowVersion, form.Id)));
        Assert.Equal(HttpStatusCode.NoContent, linkResponse.StatusCode);

        itemDetailResponse = await _client.SendAsync(
            Authorized(HttpMethod.Get, $"/api/v1/admin/website/contents/{item.Id}", accessToken));
        itemDetail = (await itemDetailResponse.Content.ReadFromJsonAsync<ContentItemDetailResponse>())!;
        var publishResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/contents/{item.Id}/publish", accessToken,
            new PublishContentItemRequest(itemDetail.RowVersion, null, null)));
        Assert.Equal(HttpStatusCode.NoContent, publishResponse.StatusCode);

        var publicResponse = await _client.GetAsync($"/api/v1/public/contents/{item.Id}?lang=tr");
        Assert.Equal(HttpStatusCode.OK, publicResponse.StatusCode);
        var publicDetail = await publicResponse.Content.ReadFromJsonAsync<PublicContentDetailResponse>();

        Assert.NotNull(publicDetail!.Form);
        Assert.Equal("İletişim Formu", publicDetail.Form!.Title);
        Assert.Single(publicDetail.Form.Fields, f => f.Key == "full_name");
        Assert.Equal(privacyNoticeKey, publicDetail.Form.PrivacyNotice.Key);
    }
}
