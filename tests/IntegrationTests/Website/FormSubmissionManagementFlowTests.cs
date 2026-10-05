using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Features.ActivateFormDefinition;
using GenclikMerkezi.Modules.Website.Features.AddFormSubmissionNote;
using GenclikMerkezi.Modules.Website.Features.ArchiveFormSubmission;
using GenclikMerkezi.Modules.Website.Features.AssignFormSubmission;
using GenclikMerkezi.Modules.Website.Features.ChangeFormSubmissionStatus;
using GenclikMerkezi.Modules.Website.Features.CreateFormDefinition;
using GenclikMerkezi.Modules.Website.Features.CreateLegalDocument;
using GenclikMerkezi.Modules.Website.Features.CreateLegalDocumentDraft;
using GenclikMerkezi.Modules.Website.Features.GetFormDefinitionById;
using GenclikMerkezi.Modules.Website.Features.GetFormSubmissionById;
using GenclikMerkezi.Modules.Website.Features.GetFormSubmissions;
using GenclikMerkezi.Modules.Website.Features.GetLegalDocumentById;
using GenclikMerkezi.Modules.Website.Features.GetPersonalDataAccessLog;
using GenclikMerkezi.Modules.Website.Features.GetSubmissionToken;
using GenclikMerkezi.Modules.Website.Features.PublishLegalDocumentDraft;
using GenclikMerkezi.Modules.Website.Features.SetFormFields;
using GenclikMerkezi.Modules.Website.Features.SubmitFormSubmission;
using GenclikMerkezi.Modules.Website.Features.UnarchiveFormSubmission;
using GenclikMerkezi.Modules.Website.Features.UpdateLegalDocumentDraftBody;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §12.2 (Faz 3 Görev 5). Shares the "one CustomWebApplicationFactory/one Sqlite database per
// class" caveat every other Website flow test class documents. Time-threshold job behavior itself
// (30-day archive window, per-form retention) is unit-tested with a fixed TimeProvider
// (ArchiveClosedFormSubmissionsJobTests/AnonymizeExpiredFormSubmissionsJobTests) - exactly like
// CleanupStaleNotFoundLogsJob's own split - since CustomWebApplicationFactory wires the real system
// TimeProvider. What this class covers instead is what only a real database can prove: the
// repository's EF query translation for GetDueForArchiveAsync/GetDueForAnonymizationAsync (exercised
// directly through the repository with a deliberately shifted "now"/threshold parameter, not by
// waiting real days) plus every admin HTTP endpoint, the access log, and real file storage round-trips.
public class FormSubmissionManagementFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly byte[] ValidPdfBytes = Encoding.ASCII.GetBytes("%PDF-1.4\n%fake pdf content for signature checks\n");

    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public FormSubmissionManagementFlowTests(CustomWebApplicationFactory factory)
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

    // Builds an active form with: required Text field "full_name", optional File field "cv".
    private async Task<string> CreateActiveFormAsync(string accessToken, string privacyNoticeKey)
    {
        var key = $"form-{Guid.NewGuid():N}";
        var createResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, "/api/v1/admin/website/forms", accessToken,
            new CreateFormDefinitionRequest(key, null, [], privacyNoticeKey, [], "Form", "Açıklama", "Teşekkürler.", "Gönder")));
        var created = (await createResponse.Content.ReadFromJsonAsync<CreateFormDefinitionResponse>())!;

        var rowVersion = (await GetFormAsync(accessToken, created.Id)).RowVersion;
        var fields = new List<FormFieldInput>
        {
            new("full_name", "Text", true, 0, null, 100, [], null, null, [], null, [new FormFieldTranslationInput("tr", "Ad Soyad", null, null)]),
            new("cv", "File", false, 1, null, null, [], null, null, ["Pdf"], 5, [new FormFieldTranslationInput("tr", "CV", null, null)]),
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

    private async Task<string> FetchSubmissionTokenAndWaitAsync()
    {
        var response = await _client.GetAsync("/api/v1/public/submission-tokens");
        var token = await response.Content.ReadFromJsonAsync<GetSubmissionTokenResponse>();
        await Task.Delay(TimeSpan.FromSeconds(3.2));
        return token!.Token;
    }

    private async Task<(string ReferenceNumber, Guid SubmissionId)> SubmitAsync(
        string formKey, int privacyVersion, string accessToken, bool withFile = false)
    {
        var token = await FetchSubmissionTokenAndWaitAsync();
        var content = new MultipartFormDataContent
        {
            { new StringContent(token), "token" },
            { new StringContent(""), "turnstileToken" },
            { new StringContent(""), "website" },
            { new StringContent("tr"), "lang" },
            { new StringContent(privacyVersion.ToString()), "acceptedPrivacyNoticeVersion" },
            { new StringContent("Test Kullanıcı"), "full_name" },
        };

        if (withFile)
        {
            var fileContent = new ByteArrayContent(ValidPdfBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            content.Add(fileContent, "cv", "cv.pdf");
        }

        var response = await _client.PostAsync($"/api/v1/public/forms/{formKey}/submissions", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = (await response.Content.ReadFromJsonAsync<SubmitFormSubmissionResponse>())!;

        var listResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, "/api/v1/admin/website/form-submissions?referenceNumber=" + result.ReferenceNumber, accessToken));
        var page = (await listResponse.Content.ReadFromJsonAsync<PagedResult<FormSubmissionSummaryResponse>>())!;
        return (result.ReferenceNumber, page.Items.Single().Id);
    }

    private async Task<(string FormKey, int PrivacyVersion, string AccessToken)> SetupActiveFormAsync()
    {
        var accessToken = await LoginAsAdminAsync();
        var (privacyKey, privacyVersion) = await CreatePublishedLegalDocumentAsync(accessToken, "PrivacyNotice");
        var formKey = await CreateActiveFormAsync(accessToken, privacyKey);
        return (formKey, privacyVersion, accessToken);
    }

    private async Task<FormSubmissionDetailResponse> GetSubmissionAsync(string accessToken, Guid id)
    {
        var response = await _client.SendAsync(Authorized(HttpMethod.Get, $"/api/v1/admin/website/form-submissions/{id}", accessToken));
        return (await response.Content.ReadFromJsonAsync<FormSubmissionDetailResponse>())!;
    }

    [Fact]
    public async Task StatusLifecycle_FollowsAllowedTransitionsAndRecordsHistory()
    {
        var (formKey, privacyVersion, accessToken) = await SetupActiveFormAsync();
        var (_, submissionId) = await SubmitAsync(formKey, privacyVersion, accessToken);

        async Task<HttpResponseMessage> ChangeStatusAsync(string status)
        {
            var detail = await GetSubmissionAsync(accessToken, submissionId);
            return await _client.SendAsync(Authorized(
                HttpMethod.Post, $"/api/v1/admin/website/form-submissions/{submissionId}/status", accessToken,
                new ChangeFormSubmissionStatusRequest(detail.RowVersion, status)));
        }

        Assert.Equal(HttpStatusCode.NoContent, (await ChangeStatusAsync("InReview")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await ChangeStatusAsync("Approved")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await ChangeStatusAsync("Completed")).StatusCode);

        var detail = await GetSubmissionAsync(accessToken, submissionId);
        Assert.Equal("Completed", detail.Status);
        Assert.NotNull(detail.ClosedAtUtc);
        Assert.Equal(3, detail.StatusHistory.Count);
        Assert.Equal("New", detail.StatusHistory[0].FromStatus);
        Assert.Equal("InReview", detail.StatusHistory[0].ToStatus);

        // Reopen from a closing status.
        Assert.Equal(HttpStatusCode.NoContent, (await ChangeStatusAsync("InReview")).StatusCode);
        detail = await GetSubmissionAsync(accessToken, submissionId);
        Assert.Equal("InReview", detail.Status);
        Assert.Null(detail.ClosedAtUtc);
    }

    [Fact]
    public async Task ChangeStatus_WithDisallowedTransition_ReturnsBadRequest()
    {
        var (formKey, privacyVersion, accessToken) = await SetupActiveFormAsync();
        var (_, submissionId) = await SubmitAsync(formKey, privacyVersion, accessToken);
        var detail = await GetSubmissionAsync(accessToken, submissionId);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/form-submissions/{submissionId}/status", accessToken,
            new ChangeFormSubmissionStatusRequest(detail.RowVersion, "Approved")));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangeStatus_WithStaleRowVersion_ReturnsConflict()
    {
        var (formKey, privacyVersion, accessToken) = await SetupActiveFormAsync();
        var (_, submissionId) = await SubmitAsync(formKey, privacyVersion, accessToken);
        var staleRowVersion = (await GetSubmissionAsync(accessToken, submissionId)).RowVersion;

        var first = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/form-submissions/{submissionId}/status", accessToken,
            new ChangeFormSubmissionStatusRequest(staleRowVersion, "InReview")));
        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);

        var second = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/form-submissions/{submissionId}/status", accessToken,
            new ChangeFormSubmissionStatusRequest(staleRowVersion, "AwaitingInfo")));
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task AssignFormSubmission_SetsAndClearsAssignee()
    {
        var (formKey, privacyVersion, accessToken) = await SetupActiveFormAsync();
        var (_, submissionId) = await SubmitAsync(formKey, privacyVersion, accessToken);
        var assigneeId = Guid.NewGuid();

        var detail = await GetSubmissionAsync(accessToken, submissionId);
        var assignResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/form-submissions/{submissionId}/assign", accessToken,
            new AssignFormSubmissionRequest(detail.RowVersion, assigneeId)));
        Assert.Equal(HttpStatusCode.NoContent, assignResponse.StatusCode);

        detail = await GetSubmissionAsync(accessToken, submissionId);
        Assert.Equal(assigneeId, detail.AssignedToUserId);

        var unassignResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/form-submissions/{submissionId}/assign", accessToken,
            new AssignFormSubmissionRequest(detail.RowVersion, null)));
        Assert.Equal(HttpStatusCode.NoContent, unassignResponse.StatusCode);

        detail = await GetSubmissionAsync(accessToken, submissionId);
        Assert.Null(detail.AssignedToUserId);
    }

    [Fact]
    public async Task AddFormSubmissionNote_AppendsNote_AndRejectsTooLongText()
    {
        var (formKey, privacyVersion, accessToken) = await SetupActiveFormAsync();
        var (_, submissionId) = await SubmitAsync(formKey, privacyVersion, accessToken);

        var detail = await GetSubmissionAsync(accessToken, submissionId);
        var addResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/form-submissions/{submissionId}/notes", accessToken,
            new AddFormSubmissionNoteRequest(detail.RowVersion, "Takip gerekiyor.")));
        Assert.Equal(HttpStatusCode.NoContent, addResponse.StatusCode);

        detail = await GetSubmissionAsync(accessToken, submissionId);
        Assert.Single(detail.InternalNotes);
        Assert.Equal("Takip gerekiyor.", detail.InternalNotes[0].Text);

        var tooLongResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/form-submissions/{submissionId}/notes", accessToken,
            new AddFormSubmissionNoteRequest(detail.RowVersion, new string('a', 2001))));
        Assert.Equal(HttpStatusCode.BadRequest, tooLongResponse.StatusCode);
    }

    [Fact]
    public async Task ArchiveAndUnarchive_OnlyWorkOnClosedSubmissions()
    {
        var (formKey, privacyVersion, accessToken) = await SetupActiveFormAsync();
        var (_, submissionId) = await SubmitAsync(formKey, privacyVersion, accessToken);

        // Cannot archive an open submission.
        var detail = await GetSubmissionAsync(accessToken, submissionId);
        var archiveTooEarly = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/form-submissions/{submissionId}/archive", accessToken,
            new ArchiveFormSubmissionRequest(detail.RowVersion)));
        Assert.Equal(HttpStatusCode.Conflict, archiveTooEarly.StatusCode);

        async Task ChangeStatusAsync(string status)
        {
            detail = await GetSubmissionAsync(accessToken, submissionId);
            var response = await _client.SendAsync(Authorized(
                HttpMethod.Post, $"/api/v1/admin/website/form-submissions/{submissionId}/status", accessToken,
                new ChangeFormSubmissionStatusRequest(detail.RowVersion, status)));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        await ChangeStatusAsync("InReview");
        await ChangeStatusAsync("Rejected");

        detail = await GetSubmissionAsync(accessToken, submissionId);
        var archiveResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/form-submissions/{submissionId}/archive", accessToken,
            new ArchiveFormSubmissionRequest(detail.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, archiveResponse.StatusCode);

        // An archived submission's status can no longer be changed directly.
        detail = await GetSubmissionAsync(accessToken, submissionId);
        Assert.NotNull(detail.ArchivedAtUtc);
        var changeWhileArchived = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/form-submissions/{submissionId}/status", accessToken,
            new ChangeFormSubmissionStatusRequest(detail.RowVersion, "InReview")));
        Assert.Equal(HttpStatusCode.Conflict, changeWhileArchived.StatusCode);

        // Excluded from the default (non-archived) list.
        var defaultListResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, "/api/v1/admin/website/form-submissions", accessToken));
        var defaultPage = (await defaultListResponse.Content.ReadFromJsonAsync<PagedResult<FormSubmissionSummaryResponse>>())!;
        Assert.DoesNotContain(defaultPage.Items, i => i.Id == submissionId);

        var archivedListResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, "/api/v1/admin/website/form-submissions?archived=true", accessToken));
        var archivedPage = (await archivedListResponse.Content.ReadFromJsonAsync<PagedResult<FormSubmissionSummaryResponse>>())!;
        Assert.Contains(archivedPage.Items, i => i.Id == submissionId);

        var unarchiveResponse = await _client.SendAsync(Authorized(
            HttpMethod.Post, $"/api/v1/admin/website/form-submissions/{submissionId}/unarchive", accessToken,
            new UnarchiveFormSubmissionRequest(detail.RowVersion)));
        Assert.Equal(HttpStatusCode.NoContent, unarchiveResponse.StatusCode);

        detail = await GetSubmissionAsync(accessToken, submissionId);
        Assert.Null(detail.ArchivedAtUtc);
    }

    [Fact]
    public async Task GetFormSubmissions_ListNeverContainsPersonalData()
    {
        var (formKey, privacyVersion, accessToken) = await SetupActiveFormAsync();
        await SubmitAsync(formKey, privacyVersion, accessToken);

        var response = await _client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/form-submissions", accessToken));
        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("Test Kullanıcı", body);
        Assert.DoesNotContain("full_name", body);
    }

    [Fact]
    public async Task GetFormSubmissions_FiltersByReferenceNumberAndStatus()
    {
        var (formKey, privacyVersion, accessToken) = await SetupActiveFormAsync();
        var (referenceNumber, submissionId) = await SubmitAsync(formKey, privacyVersion, accessToken);

        var byReferenceResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, $"/api/v1/admin/website/form-submissions?referenceNumber={referenceNumber}", accessToken));
        var byReferencePage = (await byReferenceResponse.Content.ReadFromJsonAsync<PagedResult<FormSubmissionSummaryResponse>>())!;
        Assert.Single(byReferencePage.Items);
        Assert.Equal(submissionId, byReferencePage.Items[0].Id);

        var byStatusResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, "/api/v1/admin/website/form-submissions?status=Approved", accessToken));
        var byStatusPage = (await byStatusResponse.Content.ReadFromJsonAsync<PagedResult<FormSubmissionSummaryResponse>>())!;
        Assert.DoesNotContain(byStatusPage.Items, i => i.Id == submissionId);
    }

    [Fact]
    public async Task GetFormSubmissionById_MapsAnswersToLabelsAndRecordsAccessLog()
    {
        var (formKey, privacyVersion, accessToken) = await SetupActiveFormAsync();
        var (_, submissionId) = await SubmitAsync(formKey, privacyVersion, accessToken, withFile: true);

        var detail = await GetSubmissionAsync(accessToken, submissionId);

        var answer = Assert.Single(detail.Answers, a => a.FieldKey == "full_name");
        Assert.Equal("Ad Soyad", answer.Label);
        Assert.Equal("Text", answer.FieldType);
        var file = Assert.Single(detail.Files);
        Assert.Equal("cv", file.FieldKey);
        Assert.Equal("cv.pdf", file.OriginalFileName);

        var logResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, "/api/v1/admin/website/personal-data-access-log", accessToken));
        var logPage = (await logResponse.Content.ReadFromJsonAsync<PagedResult<PersonalDataAccessLogResponse>>())!;
        Assert.Contains(logPage.Items, l => l.EntityId == submissionId && l.Action == "View");
    }

    [Fact]
    public async Task DownloadFormSubmissionFile_ReturnsFileContentAndRecordsAccessLog()
    {
        var (formKey, privacyVersion, accessToken) = await SetupActiveFormAsync();
        var (_, submissionId) = await SubmitAsync(formKey, privacyVersion, accessToken, withFile: true);
        var detail = await GetSubmissionAsync(accessToken, submissionId);
        var fileId = detail.Files[0].FileId;

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Get, $"/api/v1/admin/website/form-submissions/{submissionId}/files/{fileId}", accessToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType!.MediaType);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.Equal(ValidPdfBytes, bytes);

        var logResponse = await _client.SendAsync(Authorized(
            HttpMethod.Get, "/api/v1/admin/website/personal-data-access-log", accessToken));
        var logPage = (await logResponse.Content.ReadFromJsonAsync<PagedResult<PersonalDataAccessLogResponse>>())!;
        Assert.Contains(logPage.Items, l => l.EntityId == submissionId && l.Action == "DownloadFile");
    }

    [Fact]
    public async Task DownloadFormSubmissionFile_UnknownFileId_ReturnsNotFound()
    {
        var (formKey, privacyVersion, accessToken) = await SetupActiveFormAsync();
        var (_, submissionId) = await SubmitAsync(formKey, privacyVersion, accessToken);

        var response = await _client.SendAsync(Authorized(
            HttpMethod.Get, $"/api/v1/admin/website/form-submissions/{submissionId}/files/{Guid.NewGuid()}", accessToken));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ManagementEndpoints_WithoutAuthentication_ReturnUnauthorized()
    {
        var (formKey, privacyVersion, accessToken) = await SetupActiveFormAsync();
        var (_, submissionId) = await SubmitAsync(formKey, privacyVersion, accessToken);

        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync("/api/v1/admin/website/form-submissions")).StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized, (await _client.GetAsync($"/api/v1/admin/website/form-submissions/{submissionId}")).StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await _client.PostAsJsonAsync(
                $"/api/v1/admin/website/form-submissions/{submissionId}/status", new { rowVersion = Array.Empty<byte>(), status = "InReview" }))
            .StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized, (await _client.GetAsync("/api/v1/admin/website/personal-data-access-log")).StatusCode);
    }

    // Exercises the repository's real EF query translation (the FormSubmissions<->FormDefinitions join
    // and SubmittedAtUtc.AddDays(RetentionDays) comparison) against a real SQLite database - the
    // threshold/eligibleBefore parameter is shifted into the future instead of waiting real days, since
    // CustomWebApplicationFactory wires the real system TimeProvider (see this class's own header
    // comment). Job orchestration itself (batching, archive-after-30-days) is unit-tested separately.
    [Fact]
    public async Task FormSubmissionRepository_DueForArchiveAndAnonymization_ReflectRealElapsedWindow()
    {
        var (formKey, privacyVersion, accessToken) = await SetupActiveFormAsync();
        var (_, submissionId) = await SubmitAsync(formKey, privacyVersion, accessToken);

        async Task ChangeStatusAsync(string status)
        {
            var detail = await GetSubmissionAsync(accessToken, submissionId);
            var response = await _client.SendAsync(Authorized(
                HttpMethod.Post, $"/api/v1/admin/website/form-submissions/{submissionId}/status", accessToken,
                new ChangeFormSubmissionStatusRequest(detail.RowVersion, status)));
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        await ChangeStatusAsync("InReview");
        await ChangeStatusAsync("Rejected");

        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IFormSubmissionRepository>();

        var notYetEligible = await repository.GetDueForArchiveAsync(DateTime.UtcNow.AddDays(-1), CancellationToken.None);
        Assert.DoesNotContain(notYetEligible, s => s.Id == submissionId);

        var eligible = await repository.GetDueForArchiveAsync(DateTime.UtcNow.AddDays(31), CancellationToken.None);
        Assert.Contains(eligible, s => s.Id == submissionId);

        var notYetDueForAnonymization = await repository.GetDueForAnonymizationAsync(DateTime.UtcNow.AddDays(1), 500, CancellationToken.None);
        Assert.DoesNotContain(notYetDueForAnonymization, s => s.Id == submissionId);

        // Default RetentionDays is 730 - shift "now" well past that.
        var dueForAnonymization = await repository.GetDueForAnonymizationAsync(DateTime.UtcNow.AddDays(731), 500, CancellationToken.None);
        Assert.Contains(dueForAnonymization, s => s.Id == submissionId);
    }
}
