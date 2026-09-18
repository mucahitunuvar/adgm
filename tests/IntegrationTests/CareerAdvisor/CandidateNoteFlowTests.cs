using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.CareerAdvisor.Features.AddCandidateNote;
using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using GenclikMerkezi.Modules.CareerAdvisor.Features.GetCandidateNotes;
using GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Notification.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.CareerAdvisor;

// Görev 4/ADR-022 §3: danışmanların adaylara not düşebilmesi, "İş Görüşmesi" tipi notlarda
// senkron bildirim (email + in-app) tetiklenmesi.
public class CandidateNoteFlowTests
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

    private static async Task<(Guid AdvisorId, string AccessToken)> CreateAndLoginCareerAdvisorAsync(HttpClient client, string adminAccessToken)
    {
        var email = $"danisman-{Guid.NewGuid():N}@example.com";
        const string password = "Sifre123";

        var createRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/career-advisors")
        {
            Content = JsonContent.Create(new { email, password, firstName = "Ayşe", lastName = "Kaya", phoneNumber = (string?)null }),
        };
        createRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var createResponse = await client.SendAsync(createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateCareerAdvisorResponse>();

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        return (created!.CareerAdvisorId, login!.AccessToken);
    }

    private static async Task<(Guid CandidateCvId, Guid CandidateUserId, string AccessToken)> RegisterAndLoginCandidateAsync(HttpClient client)
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        const string password = "Sifre123";

        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/candidates/register",
            new { email, password, firstName = "Ahmet", lastName = "Yılmaz", phoneNumber = (string?)null });
        var registered = await registerResponse.Content.ReadFromJsonAsync<RegisterCandidateResponse>();

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        return (registered!.CandidateCvId, registered.UserId, login!.AccessToken);
    }

    private static Task<HttpResponseMessage> AddNoteAsync(
        HttpClient client, Guid candidateCvId, Guid candidateUserId, string noteType, string content, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/career-advisor/candidates/{candidateCvId}/notes")
        {
            Content = JsonContent.Create(new { candidateUserId, noteType, content }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client.SendAsync(request);
    }

    private static async Task<GetCandidateNotesResponse> GetNotesAsync(HttpClient client, Guid candidateCvId, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/career-advisor/candidates/{candidateCvId}/notes");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await client.SendAsync(request);
        return (await response.Content.ReadFromJsonAsync<GetCandidateNotesResponse>())!;
    }

    [Fact]
    public async Task AddNote_AsCareerAdvisor_WithGenelType_Succeeds_AndAppearsInListing()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var adminAccessToken = await LoginAsAdminAsync(factory, client);
        var (_, advisorAccessToken) = await CreateAndLoginCareerAdvisorAsync(client, adminAccessToken);
        var (candidateCvId, candidateUserId, _) = await RegisterAndLoginCandidateAsync(client);

        var response = await AddNoteAsync(client, candidateCvId, candidateUserId, "Genel", "Genel bir not", advisorAccessToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var notes = await GetNotesAsync(client, candidateCvId, advisorAccessToken);
        var note = Assert.Single(notes.Items);
        Assert.Equal("Genel", note.NoteType);
        Assert.Equal("Genel bir not", note.Content);
    }

    [Fact]
    public async Task AddNote_WithIsGorusmesiType_SendsEmailAndInAppNotification()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var adminAccessToken = await LoginAsAdminAsync(factory, client);
        var (_, advisorAccessToken) = await CreateAndLoginCareerAdvisorAsync(client, adminAccessToken);
        var (candidateCvId, candidateUserId, _) = await RegisterAndLoginCandidateAsync(client);

        var response = await AddNoteAsync(
            client, candidateCvId, candidateUserId, "IsGorusmesi", "X firması ile 10:00 görüşme", advisorAccessToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var sentEmail = Assert.Single(factory.EmailSender.SentEmails, e => e.Body.Contains("X firması ile 10:00 görüşme"));
        Assert.Contains("X firması ile 10:00 görüşme", sentEmail.Body);

        using var scope = factory.Services.CreateScope();
        var notificationDbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        var inAppNotification = notificationDbContext.InAppNotifications.SingleOrDefault(n => n.UserId == candidateUserId);
        Assert.NotNull(inAppNotification);
    }

    [Fact]
    public async Task AddNote_AsAdmin_ReturnsForbidden()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var adminAccessToken = await LoginAsAdminAsync(factory, client);
        var (candidateCvId, candidateUserId, _) = await RegisterAndLoginCandidateAsync(client);

        var response = await AddNoteAsync(client, candidateCvId, candidateUserId, "Genel", "İçerik", adminAccessToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AddNote_AsCandidate_ReturnsForbidden()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var (candidateCvId, candidateUserId, candidateAccessToken) = await RegisterAndLoginCandidateAsync(client);

        var response = await AddNoteAsync(client, candidateCvId, candidateUserId, "Genel", "İçerik", candidateAccessToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetNotes_AsAdminOrCareerAdvisor_Succeeds_AsCandidate_ReturnsForbidden()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var adminAccessToken = await LoginAsAdminAsync(factory, client);
        var (_, advisorAccessToken) = await CreateAndLoginCareerAdvisorAsync(client, adminAccessToken);
        var (candidateCvId, candidateUserId, candidateAccessToken) = await RegisterAndLoginCandidateAsync(client);
        await AddNoteAsync(client, candidateCvId, candidateUserId, "Genel", "İçerik", advisorAccessToken);

        var adminRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/career-advisor/candidates/{candidateCvId}/notes");
        adminRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminAccessToken);
        var adminResponse = await client.SendAsync(adminRequest);
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);

        var advisorRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/career-advisor/candidates/{candidateCvId}/notes");
        advisorRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);
        var advisorResponse = await client.SendAsync(advisorRequest);
        Assert.Equal(HttpStatusCode.OK, advisorResponse.StatusCode);

        var candidateRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/career-advisor/candidates/{candidateCvId}/notes");
        candidateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", candidateAccessToken);
        var candidateResponse = await client.SendAsync(candidateRequest);
        Assert.Equal(HttpStatusCode.Forbidden, candidateResponse.StatusCode);
    }
}
