using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Identity.Features.RegisterUser;

namespace GenclikMerkezi.IntegrationTests.Identity;

public class EmailVerificationFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public EmailVerificationFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RegisterThenVerify_FullFlow_ConfirmsEmailEndToEnd()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email, password = "Sifre123", role = "Candidate" });
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var registered = await registerResponse.Content.ReadFromJsonAsync<RegisterUserResponse>();

        // Register -> Outbox -> CAP (in-memory transport in Testing) -> Notification's consumer ->
        // IEmailSender is asynchronous even with the in-memory queue, so wait for it to land.
        var sentEmail = await WaitForEmailAsync(email, "doğrulayın");
        var token = ExtractTokenFromLink(sentEmail.Body);

        var loginBeforeVerify = await _client.PostAsJsonAsync(
            "/api/v1/auth/login", new { email, password = "Sifre123" });
        var loginBeforeVerifyBody = await loginBeforeVerify.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(loginBeforeVerifyBody!.EmailConfirmed);

        var verifyResponse = await _client.GetAsync($"/api/v1/auth/verify-email?token={Uri.EscapeDataString(token)}");
        Assert.Equal(HttpStatusCode.NoContent, verifyResponse.StatusCode);

        var loginAfterVerify = await _client.PostAsJsonAsync(
            "/api/v1/auth/login", new { email, password = "Sifre123" });
        var loginAfterVerifyBody = await loginAfterVerify.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.True(loginAfterVerifyBody!.EmailConfirmed);

        Assert.NotNull(registered);
    }

    [Fact]
    public async Task VerifyEmail_WithUnknownToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/auth/verify-email?token=never-issued-token");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task VerifyEmail_ViaPost_AlsoConfirmsEmail()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email, password = "Sifre123", role = "Candidate" });

        var sentEmail = await WaitForEmailAsync(email, "doğrulayın");
        var token = ExtractTokenFromLink(sentEmail.Body);

        var verifyResponse = await _client.PostAsJsonAsync("/api/v1/auth/verify-email", new { token });

        Assert.Equal(HttpStatusCode.NoContent, verifyResponse.StatusCode);
    }

    private async Task<(string ToEmail, string Subject, string Body)> WaitForEmailAsync(string toEmail, string subjectContains)
    {
        for (var i = 0; i < 100; i++)
        {
            var match = _factory.EmailSender.SentEmails
                .FirstOrDefault(e => e.ToEmail == toEmail && e.Subject.Contains(subjectContains));

            if (match != default)
            {
                return match;
            }

            await Task.Delay(50);
        }

        throw new TimeoutException($"No email to {toEmail} with subject containing '{subjectContains}' arrived in time.");
    }

    private static string ExtractTokenFromLink(string emailBody)
    {
        var match = Regex.Match(emailBody, "token=([^\"&]+)");
        Assert.True(match.Success, "Verification link with a token was not found in the email body.");
        return Uri.UnescapeDataString(match.Groups[1].Value);
    }
}
