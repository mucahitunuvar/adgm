using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Features.ActivateFormDefinition;
using GenclikMerkezi.Modules.Website.Features.CreateFormDefinition;
using GenclikMerkezi.Modules.Website.Features.CreateLegalDocument;
using GenclikMerkezi.Modules.Website.Features.CreateLegalDocumentDraft;
using GenclikMerkezi.Modules.Website.Features.GetFormDefinitionById;
using GenclikMerkezi.Modules.Website.Features.GetLegalDocumentById;
using GenclikMerkezi.Modules.Website.Features.GetPublicForm;
using GenclikMerkezi.Modules.Website.Features.GetSubmissionToken;
using GenclikMerkezi.Modules.Website.Features.PublishLegalDocumentDraft;
using GenclikMerkezi.Modules.Website.Features.SetFormFields;
using GenclikMerkezi.Modules.Website.Features.SubmitFormSubmission;
using GenclikMerkezi.Modules.Website.Features.UpdateLegalDocumentDraftBody;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §12.2 (Faz 3 Görev 4). Shares the "one CustomWebApplicationFactory/one Sqlite database per
// class" caveat every other Website flow test class documents. FakeBotProtectionVerifier (succeeds by
// default) and FakeEmailSender back this class's bot-protection and email assertions.
public class FormSubmissionFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public FormSubmissionFlowTests(CustomWebApplicationFactory factory)
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

    private async Task<(string Key, int VersionNumber)> CreatePublishedLegalDocumentAsync(string accessToken, string kind)
    {
        var key = $"legal-{Guid.NewGuid():N}";
        var createResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/legal-documents", accessToken,
            new CreateLegalDocumentRequest(key, kind, "Başlık")));
        var created = (await createResponse.Content.ReadFromJsonAsync<CreateLegalDocumentResponse>())!;

        var detail = await GetLegalDocumentAsync(accessToken, created.Id);
        await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/legal-documents/{created.Id}/versions/draft", accessToken,
            new CreateLegalDocumentDraftRequest(detail.RowVersion, null)));

        detail = await GetLegalDocumentAsync(accessToken, created.Id);
        await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/legal-documents/{created.Id}/versions/draft/translations/tr", accessToken,
            new UpdateLegalDocumentDraftBodyRequest(detail.RowVersion, "<p>Gövde</p>")));

        detail = await GetLegalDocumentAsync(accessToken, created.Id);
        var publishResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/legal-documents/{created.Id}/versions/draft/publish", accessToken,
            new PublishLegalDocumentDraftRequest(detail.RowVersion, null)));
        Assert.Equal(HttpStatusCode.NoContent, publishResponse.StatusCode);

        detail = await GetLegalDocumentAsync(accessToken, created.Id);
        return (key, detail.EffectiveVersionNumber!.Value);
    }

    private async Task<LegalDocumentDetailResponse> GetLegalDocumentAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/legal-documents/{id}", accessToken));
        return (await response.Content.ReadFromJsonAsync<LegalDocumentDetailResponse>())!;
    }

    private async Task<FormDefinitionDetailResponse> GetFormAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/forms/{id}", accessToken));
        return (await response.Content.ReadFromJsonAsync<FormDefinitionDetailResponse>())!;
    }

    // Builds an active form with: required Text field "full_name", optional Email field "email",
    // optional File field "cv" (Pdf only, max 5MB), one required explicit consent (if consentKey given).
    private async Task<string> CreateActiveFormAsync(
        string accessToken, string privacyNoticeKey, string? consentKey = null, string? notificationEmail = null)
    {
        var key = $"form-{Guid.NewGuid():N}";
        var explicitConsents = consentKey is null
            ? []
            : new List<CreateFormDefinitionExplicitConsentInput> { new(consentKey, true) };
        var notificationEmails = notificationEmail is null ? [] : new List<string> { notificationEmail };

        var createResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/forms", accessToken,
            new CreateFormDefinitionRequest(
                key, null, notificationEmails, privacyNoticeKey, explicitConsents, "İletişim Formu", "Açıklama", "Teşekkürler, başvurunuz alındı.",
                "Gönder")));
        var created = (await createResponse.Content.ReadFromJsonAsync<CreateFormDefinitionResponse>())!;

        var rowVersion = (await GetFormAsync(accessToken, created.Id)).RowVersion;
        var fields = new List<FormFieldInput>
        {
            new("full_name", "Text", true, 0, null, 100, [], null, null, [], null, [new FormFieldTranslationInput("tr", "Ad Soyad", null, null)]),
            new("email", "Email", false, 1, null, null, [], null, null, [], null, [new FormFieldTranslationInput("tr", "E-posta", null, null)]),
            new("cv", "File", false, 2, null, null, [], null, null, ["Pdf"], 5, [new FormFieldTranslationInput("tr", "CV", null, null)]),
        };
        var setFieldsResponse = await _client.SendAsync(Authorized(
            HttpMethod.Put, $"/api/v1/admin/website/forms/{created.Id}/fields", accessToken, new SetFormFieldsRequest(rowVersion, fields)));
        Assert.Equal(HttpStatusCode.NoContent, setFieldsResponse.StatusCode);

        rowVersion = (await GetFormAsync(accessToken, created.Id)).RowVersion;
        var activateResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/forms/{created.Id}/activate", accessToken, new ActivateFormDefinitionRequest(rowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, activateResponse.StatusCode);

        return key;
    }

    private async Task<string> FetchSubmissionTokenAsync()
    {
        var response = await _client.GetAsync("/api/v1/public/submission-tokens");
        var token = await response.Content.ReadFromJsonAsync<GetSubmissionTokenResponse>();
        return token!.Token;
    }

    // Waits past PublicSubmissionGuard.MinimumFillDuration (3s) so a token fetched just before calling
    // this is old enough to pass the guard's anti-bot timing check.
    private static async Task<string> FetchSubmissionTokenAndWaitAsync(FormSubmissionFlowTests self)
    {
        var token = await self.FetchSubmissionTokenAsync();
        await Task.Delay(TimeSpan.FromSeconds(3.2));
        return token;
    }

    private static MultipartFormDataContent BuildSubmissionContent(
        string token, int acceptedPrivacyNoticeVersion, IReadOnlyDictionary<string, string>? answers = null,
        IReadOnlyList<(string Key, int Version, bool Accepted)>? explicitConsents = null, byte[]? cvFileBytes = null,
        string cvFileName = "cv.pdf", string cvContentType = "application/pdf")
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent(token), "token" },
            { new StringContent(""), "turnstileToken" },
            { new StringContent(""), "website" },
            { new StringContent("tr"), "lang" },
            { new StringContent(acceptedPrivacyNoticeVersion.ToString()), "acceptedPrivacyNoticeVersion" },
        };

        if (explicitConsents is not null)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(
                explicitConsents.Select(c => new { key = c.Key, version = c.Version, accepted = c.Accepted }));
            content.Add(new StringContent(json), "explicitConsents");
        }

        foreach (var (key, value) in answers ?? new Dictionary<string, string>())
        {
            content.Add(new StringContent(value), key);
        }

        if (cvFileBytes is not null)
        {
            var fileContent = new ByteArrayContent(cvFileBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(cvContentType);
            content.Add(fileContent, "cv", cvFileName);
        }

        return content;
    }

    private static readonly byte[] ValidPdfBytes = Encoding.ASCII.GetBytes("%PDF-1.4\n%fake pdf content for signature checks\n");

    [Fact]
    public async Task GetPublicForm_ForActiveForm_ReturnsDefinitionWithFieldsAndPrivacyNotice()
    {
        var accessToken = await LoginAsAdminAsync();
        var (privacyKey, privacyVersion) = await CreatePublishedLegalDocumentAsync(accessToken, "PrivacyNotice");
        var formKey = await CreateActiveFormAsync(accessToken, privacyKey);

        var response = await _client.GetAsync($"/api/v1/public/forms/{formKey}?lang=tr");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var definition = await response.Content.ReadFromJsonAsync<PublicFormDefinitionResponseDto>();
        Assert.Equal("İletişim Formu", definition!.Title);
        Assert.Equal(3, definition.Fields.Count);
        Assert.Equal(privacyKey, definition.PrivacyNotice.Key);
        Assert.Equal(privacyVersion, definition.PrivacyNotice.VersionNumber);
    }

    [Fact]
    public async Task GetPublicForm_ForUnknownKey_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/v1/public/forms/unknown-{Guid.NewGuid():N}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SubmitFormSubmission_WithValidData_SucceedsAndSendsEmailsWithoutSubmissionContent()
    {
        var accessToken = await LoginAsAdminAsync();
        var (privacyKey, privacyVersion) = await CreatePublishedLegalDocumentAsync(accessToken, "PrivacyNotice");
        var notificationEmail = $"notify-{Guid.NewGuid():N}@example.com";
        var formKey = await CreateActiveFormAsync(accessToken, privacyKey, notificationEmail: notificationEmail);

        var token = await FetchSubmissionTokenAndWaitAsync(this);
        var submitterEmail = $"submitter-{Guid.NewGuid():N}@example.com";
        var answers = new Dictionary<string, string> { ["full_name"] = "Gizli Ad Soyad Bilgisi", ["email"] = submitterEmail };

        var content = BuildSubmissionContent(token, privacyVersion, answers, cvFileBytes: ValidPdfBytes);
        var response = await _client.PostAsync($"/api/v1/public/forms/{formKey}/submissions", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<SubmitFormSubmissionResponse>();
        Assert.Matches(@"^GM-\d{4}-\d{6}$", result!.ReferenceNumber);
        Assert.Equal("Teşekkürler, başvurunuz alındı.", result.SuccessMessage);

        var confirmationEmail = Assert.Single(_factory.EmailSender.SentEmails, e => e.ToEmail == submitterEmail);
        Assert.Contains(result.ReferenceNumber, confirmationEmail.Body);
        Assert.DoesNotContain("Gizli Ad Soyad Bilgisi", confirmationEmail.Body);

        var notification = Assert.Single(_factory.EmailSender.SentEmails, e => e.ToEmail == notificationEmail);
        Assert.Contains(result.ReferenceNumber, notification.Body);
        Assert.DoesNotContain("Gizli Ad Soyad Bilgisi", notification.Body);
        Assert.DoesNotContain(submitterEmail, notification.Body);
    }

    [Fact]
    public async Task SubmitFormSubmission_ValidationScenarios_AreRejected()
    {
        var accessToken = await LoginAsAdminAsync();
        var (privacyKey, privacyVersion) = await CreatePublishedLegalDocumentAsync(accessToken, "PrivacyNotice");
        var (consentKey, consentVersion) = await CreatePublishedLegalDocumentAsync(accessToken, "ExplicitConsent");
        var formKey = await CreateActiveFormAsync(accessToken, privacyKey, consentKey);

        var token = await FetchSubmissionTokenAndWaitAsync(this);
        var validAnswers = new Dictionary<string, string> { ["full_name"] = "Test Kullanıcı" };
        var validConsents = new List<(string, int, bool)> { (consentKey, consentVersion, true) };

        // Stale privacy notice version.
        var staleVersionContent = BuildSubmissionContent(token, privacyVersion + 1, validAnswers, validConsents);
        var staleVersionResponse = await _client.PostAsync($"/api/v1/public/forms/{formKey}/submissions", staleVersionContent);
        Assert.Equal(HttpStatusCode.Conflict, staleVersionResponse.StatusCode);

        // Required consent not accepted.
        var missingConsentContent = BuildSubmissionContent(token, privacyVersion, validAnswers, []);
        var missingConsentResponse = await _client.PostAsync($"/api/v1/public/forms/{formKey}/submissions", missingConsentContent);
        Assert.Equal(HttpStatusCode.BadRequest, missingConsentResponse.StatusCode);

        // Required field missing.
        var missingFieldContent = BuildSubmissionContent(token, privacyVersion, new Dictionary<string, string>(), validConsents);
        var missingFieldResponse = await _client.PostAsync($"/api/v1/public/forms/{formKey}/submissions", missingFieldContent);
        Assert.Equal(HttpStatusCode.BadRequest, missingFieldResponse.StatusCode);

        // Unknown field key.
        var unknownFieldAnswers = new Dictionary<string, string>(validAnswers) { ["not_a_field"] = "x" };
        var unknownFieldContent = BuildSubmissionContent(token, privacyVersion, unknownFieldAnswers, validConsents);
        var unknownFieldResponse = await _client.PostAsync($"/api/v1/public/forms/{formKey}/submissions", unknownFieldContent);
        Assert.Equal(HttpStatusCode.BadRequest, unknownFieldResponse.StatusCode);

        // Invalid email format.
        var invalidEmailAnswers = new Dictionary<string, string>(validAnswers) { ["email"] = "not-an-email" };
        var invalidEmailContent = BuildSubmissionContent(token, privacyVersion, invalidEmailAnswers, validConsents);
        var invalidEmailResponse = await _client.PostAsync($"/api/v1/public/forms/{formKey}/submissions", invalidEmailContent);
        Assert.Equal(HttpStatusCode.BadRequest, invalidEmailResponse.StatusCode);

        // File with wrong signature (claims to be a PDF, is not).
        var fakeFileBytes = Encoding.ASCII.GetBytes("this is definitely not a pdf");
        var badSignatureContent = BuildSubmissionContent(token, privacyVersion, validAnswers, validConsents, fakeFileBytes);
        var badSignatureResponse = await _client.PostAsync($"/api/v1/public/forms/{formKey}/submissions", badSignatureContent);
        Assert.Equal(HttpStatusCode.BadRequest, badSignatureResponse.StatusCode);
    }

    [Fact]
    public async Task SubmitFormSubmission_WithoutSubmissionToken_IsRejected()
    {
        var accessToken = await LoginAsAdminAsync();
        var (privacyKey, privacyVersion) = await CreatePublishedLegalDocumentAsync(accessToken, "PrivacyNotice");
        var formKey = await CreateActiveFormAsync(accessToken, privacyKey);

        var answers = new Dictionary<string, string> { ["full_name"] = "Test Kullanıcı" };
        var content = BuildSubmissionContent(string.Empty, privacyVersion, answers);

        var response = await _client.PostAsync($"/api/v1/public/forms/{formKey}/submissions", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("token", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SubmitFormSubmission_ConcurrentSubmissions_ProduceUniqueSequentialReferenceNumbers()
    {
        var accessToken = await LoginAsAdminAsync();
        var (privacyKey, privacyVersion) = await CreatePublishedLegalDocumentAsync(accessToken, "PrivacyNotice");
        var formKey = await CreateActiveFormAsync(accessToken, privacyKey);

        var token = await FetchSubmissionTokenAndWaitAsync(this);
        var answers = new Dictionary<string, string> { ["full_name"] = "Eşzamanlı Test" };

        var tasks = Enumerable.Range(0, 10).Select(async _ =>
        {
            var content = BuildSubmissionContent(token, privacyVersion, answers);
            var response = await _client.PostAsync($"/api/v1/public/forms/{formKey}/submissions", content);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<SubmitFormSubmissionResponse>())!.ReferenceNumber;
        });

        var referenceNumbers = await Task.WhenAll(tasks);

        Assert.Equal(10, referenceNumbers.Distinct().Count());
        var sequenceValues = referenceNumbers
            .Select(r => int.Parse(r.Split('-')[2]))
            .OrderBy(v => v)
            .ToList();
        Assert.Equal(Enumerable.Range(sequenceValues[0], 10), sequenceValues);
    }

    private sealed record PublicFormDefinitionResponseDto(
        string Key, string Title, string Description, string SubmitButtonLabel, List<PublicFormFieldResponseDto> Fields,
        PublicFormLegalDocumentReferenceResponseDto PrivacyNotice, List<PublicFormLegalDocumentReferenceResponseDto> ExplicitConsents);

    private sealed record PublicFormFieldResponseDto(string Key, string Type);

    private sealed record PublicFormLegalDocumentReferenceResponseDto(string Key, int VersionNumber, string Title, bool IsRequired);
}
