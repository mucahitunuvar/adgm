using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;
using GenclikMerkezi.Modules.Candidate.Features.SendBulkCandidateNotification;
using GenclikMerkezi.Modules.Identity.Features.Login;

namespace GenclikMerkezi.IntegrationTests.CareerAdvisor;

// Görev 8/ADR-022 §6: danışmanın kendi adaylarının tamamına (veya seçili alt kümesine) toplu
// bildirim gönderebilmesi. Yetkilendirme/alıcı-listesi çözümlemesi Candidate modülünde yaşar
// (CandidateCv.CareerAdvisorId'ye yalnızca o sahip); gerçek gönderim CareerAdvisor → Notification
// yönünde gerçekleşir.
public class BulkNotificationFlowTests
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

    private static async Task<(Guid CandidateCvId, string Email, string AccessToken)> RegisterAndLoginCandidateAsync(HttpClient client)
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        const string password = "Sifre123";

        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/candidates/register",
            new { email, password, firstName = "Ahmet", lastName = "Yılmaz", phoneNumber = (string?)null });
        var registered = await registerResponse.Content.ReadFromJsonAsync<RegisterCandidateResponse>();

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        return (registered!.CandidateCvId, email, login!.AccessToken);
    }

    private static Task<HttpResponseMessage> SendBulkNotificationAsync(
        HttpClient client, IReadOnlyList<Guid>? candidateCvIds, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/candidates/bulk-notifications")
        {
            Content = JsonContent.Create(new { candidateCvIds, subject = "Duyuru", message = "Önemli bir duyuru" }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client.SendAsync(request);
    }

    [Fact]
    public async Task SendBulkNotification_WithNoExplicitIds_NotifiesAllOwnCandidates()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var adminAccessToken = await LoginAsAdminAsync(factory, client);
        var (_, advisorAccessToken) = await CreateAndLoginCareerAdvisorAsync(client, adminAccessToken);
        // Tek danışman aktif olduğu için en-az-yüklü atama üçünü de bu danışmana bağlar.
        var (_, email1, _) = await RegisterAndLoginCandidateAsync(client);
        var (_, email2, _) = await RegisterAndLoginCandidateAsync(client);
        var (_, email3, _) = await RegisterAndLoginCandidateAsync(client);

        var response = await SendBulkNotificationAsync(client, null, advisorAccessToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<SendBulkCandidateNotificationResponse>();
        Assert.Equal(3, body!.RecipientCount);
        Assert.Contains(factory.EmailSender.SentEmails, e => e.ToEmail == email1 && e.Subject == "Duyuru");
        Assert.Contains(factory.EmailSender.SentEmails, e => e.ToEmail == email2 && e.Subject == "Duyuru");
        Assert.Contains(factory.EmailSender.SentEmails, e => e.ToEmail == email3 && e.Subject == "Duyuru");
    }

    [Fact]
    public async Task SendBulkNotification_WithExplicitSubset_NotifiesOnlyThoseCandidates()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var adminAccessToken = await LoginAsAdminAsync(factory, client);
        var (_, advisorAccessToken) = await CreateAndLoginCareerAdvisorAsync(client, adminAccessToken);
        var (candidateCvId1, email1, _) = await RegisterAndLoginCandidateAsync(client);
        var (_, email2, _) = await RegisterAndLoginCandidateAsync(client);

        var response = await SendBulkNotificationAsync(client, [candidateCvId1], advisorAccessToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<SendBulkCandidateNotificationResponse>();
        Assert.Equal(1, body!.RecipientCount);
        Assert.Contains(factory.EmailSender.SentEmails, e => e.ToEmail == email1 && e.Subject == "Duyuru");
        Assert.DoesNotContain(factory.EmailSender.SentEmails, e => e.ToEmail == email2 && e.Subject == "Duyuru");
    }

    [Fact]
    public async Task SendBulkNotification_WithAnotherAdvisorsCandidateId_ExcludesIt()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var adminAccessToken = await LoginAsAdminAsync(factory, client);
        // Tek danışman -> aday buna bağlanır. "Diğer" danışman kayıttan SONRA oluşturulur ki
        // hiçbir adaya sahip olmasın (MeetingRequestFlowTests'teki aynı determinizm gerekçesi).
        var (_, ownerAdvisorAccessToken) = await CreateAndLoginCareerAdvisorAsync(client, adminAccessToken);
        var (ownedCandidateCvId, ownedEmail, _) = await RegisterAndLoginCandidateAsync(client);
        var (_, otherAdvisorAccessToken) = await CreateAndLoginCareerAdvisorAsync(client, adminAccessToken);

        var response = await SendBulkNotificationAsync(client, [ownedCandidateCvId], otherAdvisorAccessToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.DoesNotContain(factory.EmailSender.SentEmails, e => e.ToEmail == ownedEmail && e.Subject == "Duyuru");
        _ = ownerAdvisorAccessToken;
    }

    [Fact]
    public async Task SendBulkNotification_AsCandidate_ReturnsForbidden()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var (_, _, candidateAccessToken) = await RegisterAndLoginCandidateAsync(client);

        var response = await SendBulkNotificationAsync(client, null, candidateAccessToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
