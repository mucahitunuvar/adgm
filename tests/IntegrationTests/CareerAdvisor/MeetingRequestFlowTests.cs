using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;
using GenclikMerkezi.Modules.Candidate.Features.RequestMeeting;
using GenclikMerkezi.Modules.Identity.Features.Login;

namespace GenclikMerkezi.IntegrationTests.CareerAdvisor;

// Görev 5/ADR-022 §4: aday-başlatmalı görüşme talebi uçtan uca akışı (talep → tarih önerisi →
// onay/reddetme), her adımda ilgili tarafa senkron bildirim (email + in-app).
public class MeetingRequestFlowTests
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

    private static async Task<(Guid CandidateCvId, string AccessToken)> RegisterAndLoginCandidateAsync(HttpClient client)
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        const string password = "Sifre123";

        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/candidates/register",
            new { email, password, firstName = "Ahmet", lastName = "Yılmaz", phoneNumber = (string?)null });
        var registered = await registerResponse.Content.ReadFromJsonAsync<RegisterCandidateResponse>();

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        return (registered!.CandidateCvId, login!.AccessToken);
    }

    private static async Task<Guid> RequestMeetingAsync(HttpClient client, Guid candidateCvId, string candidateAccessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/candidates/{candidateCvId}/meeting-requests");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", candidateAccessToken);
        var response = await client.SendAsync(request);
        var body = await response.Content.ReadFromJsonAsync<RequestMeetingResponse>();
        return body!.MeetingRequestId;
    }

    private static Task<HttpResponseMessage> ProposeMeetingTimeAsync(
        HttpClient client, Guid meetingRequestId, DateTime proposedDateTimeUtc, string advisorAccessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/career-advisor/meeting-requests/{meetingRequestId}/propose-time")
        {
            Content = JsonContent.Create(new { proposedDateTimeUtc }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);
        return client.SendAsync(request);
    }

    private static Task<HttpResponseMessage> RejectMeetingRequestAsync(HttpClient client, Guid meetingRequestId, string advisorAccessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/career-advisor/meeting-requests/{meetingRequestId}/reject");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", advisorAccessToken);
        return client.SendAsync(request);
    }

    private static Task<HttpResponseMessage> ConfirmMeetingAsync(
        HttpClient client, Guid candidateCvId, Guid meetingRequestId, string candidateAccessToken)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post, $"/api/v1/candidates/{candidateCvId}/meeting-requests/{meetingRequestId}/confirm");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", candidateAccessToken);
        return client.SendAsync(request);
    }

    [Fact]
    public async Task FullFlow_RequestThenProposeThenConfirm_Succeeds_WithNotificationsAtEachStep()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var adminAccessToken = await LoginAsAdminAsync(factory, client);
        var (_, advisorAccessToken) = await CreateAndLoginCareerAdvisorAsync(client, adminAccessToken);
        var (candidateCvId, candidateAccessToken) = await RegisterAndLoginCandidateAsync(client);

        // Aday talep açar -> danışmana bildirim
        var meetingRequestId = await RequestMeetingAsync(client, candidateCvId, candidateAccessToken);
        Assert.NotEqual(Guid.Empty, meetingRequestId);
        Assert.NotEmpty(factory.EmailSender.SentEmails);

        var emailCountAfterRequest = factory.EmailSender.SentEmails.Count;

        // Danışman tarih önerir -> adaya bildirim
        var proposedDateTimeUtc = DateTime.UtcNow.AddDays(3);
        var proposeResponse = await ProposeMeetingTimeAsync(client, meetingRequestId, proposedDateTimeUtc, advisorAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, proposeResponse.StatusCode);
        Assert.True(factory.EmailSender.SentEmails.Count > emailCountAfterRequest);

        var emailCountAfterPropose = factory.EmailSender.SentEmails.Count;

        // Aday onaylar -> danışmana bildirim
        var confirmResponse = await ConfirmMeetingAsync(client, candidateCvId, meetingRequestId, candidateAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, confirmResponse.StatusCode);
        Assert.True(factory.EmailSender.SentEmails.Count > emailCountAfterPropose);
    }

    [Fact]
    public async Task RejectFlow_RequestThenReject_Succeeds_AndBlocksLaterConfirm()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var adminAccessToken = await LoginAsAdminAsync(factory, client);
        var (_, advisorAccessToken) = await CreateAndLoginCareerAdvisorAsync(client, adminAccessToken);
        var (candidateCvId, candidateAccessToken) = await RegisterAndLoginCandidateAsync(client);
        var meetingRequestId = await RequestMeetingAsync(client, candidateCvId, candidateAccessToken);

        var rejectResponse = await RejectMeetingRequestAsync(client, meetingRequestId, advisorAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, rejectResponse.StatusCode);

        // Reddedilmiş bir talebi onaylamaya çalışmak (geçersiz durum geçişi) -> 409
        var confirmResponse = await ConfirmMeetingAsync(client, candidateCvId, meetingRequestId, candidateAccessToken);
        Assert.Equal(HttpStatusCode.Conflict, confirmResponse.StatusCode);
    }

    [Fact]
    public async Task ProposeMeetingTime_AsAnotherAdvisor_ReturnsForbidden()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var adminAccessToken = await LoginAsAdminAsync(factory, client);
        // Kaydolduğunda ortada tek aktif danışman var (owner) - en-az-yüklü atama (Görev 2) adayı
        // kesin olarak owner'a bağlar. "other" danışman ancak kayıttan SONRA oluşturuluyor, ki
        // atamaya hiç aday olamasın ve gerçekten "başka bir danışman" olduğu garanti olsun.
        var (_, ownerAdvisorAccessToken) = await CreateAndLoginCareerAdvisorAsync(client, adminAccessToken);
        var (candidateCvId, candidateAccessToken) = await RegisterAndLoginCandidateAsync(client);
        var meetingRequestId = await RequestMeetingAsync(client, candidateCvId, candidateAccessToken);
        var (_, otherAdvisorAccessToken) = await CreateAndLoginCareerAdvisorAsync(client, adminAccessToken);
        _ = ownerAdvisorAccessToken;

        var response = await ProposeMeetingTimeAsync(client, meetingRequestId, DateTime.UtcNow.AddDays(1), otherAdvisorAccessToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmMeeting_AsAnotherCandidate_ReturnsForbidden()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var adminAccessToken = await LoginAsAdminAsync(factory, client);
        var (_, advisorAccessToken) = await CreateAndLoginCareerAdvisorAsync(client, adminAccessToken);
        var (candidateCvId, candidateAccessToken) = await RegisterAndLoginCandidateAsync(client);
        var meetingRequestId = await RequestMeetingAsync(client, candidateCvId, candidateAccessToken);
        await ProposeMeetingTimeAsync(client, meetingRequestId, DateTime.UtcNow.AddDays(1), advisorAccessToken);

        var (otherCandidateCvId, otherCandidateAccessToken) = await RegisterAndLoginCandidateAsync(client);

        var response = await ConfirmMeetingAsync(client, otherCandidateCvId, meetingRequestId, otherCandidateAccessToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RequestMeeting_WithNoAssignedAdvisor_ReturnsConflict()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var (candidateCvId, candidateAccessToken) = await RegisterAndLoginCandidateAsync(client);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/candidates/{candidateCvId}/meeting-requests");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", candidateAccessToken);
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
