using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace GenclikMerkezi.IntegrationTests.Identity;

public class PasswordResetFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PasswordResetFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ForgotPasswordThenReset_FullFlow_ChangesPasswordEndToEnd()
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync(
            "/api/v1/auth/register", new { email, password = "OldPass123", role = "Candidate" });

        var forgotResponse = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email });
        Assert.Equal(HttpStatusCode.NoContent, forgotResponse.StatusCode);

        // Register -> Outbox -> CAP (in-memory transport in Testing) -> Notification's consumer ->
        // IEmailSender is asynchronous even with the in-memory queue, so wait for it to land.
        var sentEmail = await WaitForEmailAsync(email, "Şifre sıfırlama");
        var token = ExtractResetCode(sentEmail.Body);

        var resetResponse = await _client.PostAsJsonAsync(
            "/api/v1/auth/reset-password", new { token, newPassword = "NewPass456" });
        Assert.Equal(HttpStatusCode.NoContent, resetResponse.StatusCode);

        var loginWithOldPassword = await _client.PostAsJsonAsync(
            "/api/v1/auth/login", new { email, password = "OldPass123" });
        Assert.Equal(HttpStatusCode.Unauthorized, loginWithOldPassword.StatusCode);

        var loginWithNewPassword = await _client.PostAsJsonAsync(
            "/api/v1/auth/login", new { email, password = "NewPass456" });
        Assert.Equal(HttpStatusCode.OK, loginWithNewPassword.StatusCode);
    }

    [Fact]
    public async Task ForgotPassword_WithUnknownEmail_StillReturnsNoContent_AndSendsNoEmail()
    {
        var unknownEmail = $"unknown-{Guid.NewGuid():N}@example.com";

        var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email = unknownEmail });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await Task.Delay(200);
        Assert.DoesNotContain(_factory.EmailSender.SentEmails, e => e.ToEmail == unknownEmail);
    }

    [Fact]
    public async Task ResetPassword_WithUnknownToken_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/auth/reset-password", new { token = "never-issued-token", newPassword = "NewPass456" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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

    private static string ExtractResetCode(string emailBody)
    {
        var match = Regex.Match(emailBody, "<strong>([^<]+)</strong>");
        Assert.True(match.Success, "Reset code was not found in the email body.");
        return match.Groups[1].Value;
    }
}
