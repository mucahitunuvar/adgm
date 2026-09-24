using GenclikMerkezi.Contracts.Website;
using GenclikMerkezi.IntegrationTests.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §1 Görev 7: no Website feature calls IWebsiteEmailSender yet (email-sending features
// arrive in later Faz'lar), so nothing else in this suite exercises the DI wiring across Website's
// port, the Host's NotificationWebsiteEmailSender adapter and Notification's public contract. This
// resolves the port directly from the composed Host container, the same way a future feature's
// handler would via constructor injection, and confirms the whole chain actually delivers.
public class WebsiteEmailSenderTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public WebsiteEmailSenderTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SendEmailAsync_ResolvedFromContainer_DeliversThroughNotificationModule()
    {
        using var scope = _factory.Services.CreateScope();
        var websiteEmailSender = scope.ServiceProvider.GetRequiredService<IWebsiteEmailSender>();

        await websiteEmailSender.SendEmailAsync("ziyaretci@example.com", "Etkinlik Kaydınız Onaylandı", "<p>Merhaba</p>");

        var sentEmail = Assert.Single(_factory.EmailSender.SentEmails, e => e.ToEmail == "ziyaretci@example.com");
        Assert.Equal("Etkinlik Kaydınız Onaylandı", sentEmail.Subject);
        Assert.Equal("<p>Merhaba</p>", sentEmail.Body);
    }
}
