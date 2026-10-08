using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Features.GetSiteSettings;
using GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §10 (Faz 5 Görev 4). Drives EmployerJobSearchSource (the Host-level adapter wired to
// IPublishedJobModuleContract) through a fake contract (EmployerJobSearchSourceTestFactory) instead of
// a real Company/Job review workflow - "sahte contract" per the master prompt's own test list.
public class EmployerJobSearchSourceFlowTests
{
    private static PublishedJobSummary NewJob(string suffix, DateTime publishedAtUtc) => new(
        Guid.NewGuid(), $"job-{suffix}", $"Title {suffix}", "ACME A.Ş.", "İstanbul", "Tam Zamanlı", "Hibrit", "Yazılım Mühendisi",
        $"Summary {suffix}", publishedAtUtc);

    private async Task<string> LoginAsAdminAsync(EmployerJobSearchSourceTestFactory factory, HttpClient client)
    {
        var email = $"admin-{Guid.NewGuid():N}@example.com";
        const string password = "AdminSifre123";
        await factory.SeedAdminUserAsync(email, password);

        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return login!.AccessToken;
    }

    private static HttpRequestMessage Authorized(HttpMethod method, string path, string accessToken, object? body = null)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }

    private async Task SetPublicJobListingsEnabledAsync(HttpClient client, string accessToken, bool enabled)
    {
        var getResponse = await client.SendAsync(Authorized(HttpMethod.Get, "/api/v1/admin/website/settings", accessToken));
        var settings = (await getResponse.Content.ReadFromJsonAsync<SiteSettingsResponse>())!;

        var putResponse = await client.SendAsync(Authorized(
            HttpMethod.Put, "/api/v1/admin/website/settings/features", accessToken,
            new UpdateSiteSettingsFeaturesRequest(
                settings.RowVersion, settings.GlobalSearchEnabled, settings.NewsletterEnabled, enabled, settings.DonationPageEnabled)));
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);
    }

    private static IExternalSearchSource ResolveEmployerJobSource(IServiceProvider services) =>
        services.GetServices<IExternalSearchSource>().Single(s => s.SourceKey == "employer.job");

    [Fact]
    public async Task GetPublishedDocumentsAsync_MapsEveryField_AsSpecified()
    {
        using var factory = new EmployerJobSearchSourceTestFactory();
        var client = factory.CreateClient();
        var accessToken = await LoginAsAdminAsync(factory, client);
        await SetPublicJobListingsEnabledAsync(client, accessToken, enabled: true);

        var job = NewJob("mapping", DateTime.UtcNow);
        factory.FakeContract.Jobs.Add(job);

        using var scope = factory.Services.CreateScope();
        var source = ResolveEmployerJobSource(scope.ServiceProvider);
        var page = await source.GetPublishedDocumentsAsync(1, 200);

        Assert.Equal("employer.job", source.SourceKey);
        var document = Assert.Single(page.Items);
        Assert.Equal(job.JobId.ToString(), document.SourceId);
        Assert.Equal("job", document.TypeKey);
        Assert.Equal(job.Title, document.Title);
        Assert.Equal(job.Summary, document.Summary);
        Assert.Equal($"/ilanlar/{job.Slug}", document.Url);
        Assert.Equal(job.PublishedAtUtc, document.PublishedAtUtc);
        Assert.True(document.IncludeInSitemap);
        Assert.Contains(job.Title, document.SearchableText, StringComparison.Ordinal);
        Assert.Contains(job.CompanyName, document.SearchableText, StringComparison.Ordinal);
        Assert.Contains(job.ProvinceName!, document.SearchableText, StringComparison.Ordinal);
        Assert.Contains(job.PositionName!, document.SearchableText, StringComparison.Ordinal);
        Assert.Contains(job.EmploymentTypeName!, document.SearchableText, StringComparison.Ordinal);
        Assert.Contains(job.WorkLocationTypeName!, document.SearchableText, StringComparison.Ordinal);
        Assert.Contains(job.Summary, document.SearchableText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetPublishedDocumentsAsync_WhenPublicJobListingsDisabled_ReturnsEmptyPage()
    {
        using var factory = new EmployerJobSearchSourceTestFactory();
        var client = factory.CreateClient();
        var accessToken = await LoginAsAdminAsync(factory, client);
        await SetPublicJobListingsEnabledAsync(client, accessToken, enabled: false);

        factory.FakeContract.Jobs.Add(NewJob("disabled", DateTime.UtcNow));

        using var scope = factory.Services.CreateScope();
        var source = ResolveEmployerJobSource(scope.ServiceProvider);
        var page = await source.GetPublishedDocumentsAsync(1, 200);

        Assert.Empty(page.Items);
        Assert.Equal(0, page.TotalCount);
    }

    [Fact]
    public async Task GetPublishedDocumentsAsync_With250Jobs_PaginatesAcrossTwoPagesWithoutGapsOrDuplicates()
    {
        using var factory = new EmployerJobSearchSourceTestFactory();
        var client = factory.CreateClient();
        var accessToken = await LoginAsAdminAsync(factory, client);
        await SetPublicJobListingsEnabledAsync(client, accessToken, enabled: true);

        var now = DateTime.UtcNow;
        for (var i = 0; i < 250; i++)
        {
            factory.FakeContract.Jobs.Add(NewJob($"page-{i}", now.AddMinutes(-i)));
        }

        using var scope = factory.Services.CreateScope();
        var source = ResolveEmployerJobSource(scope.ServiceProvider);

        var firstPage = await source.GetPublishedDocumentsAsync(1, 200);
        var secondPage = await source.GetPublishedDocumentsAsync(2, 200);

        Assert.Equal(250, firstPage.TotalCount);
        Assert.Equal(200, firstPage.Items.Count);
        Assert.Equal(50, secondPage.Items.Count);

        var allSourceIds = firstPage.Items.Concat(secondPage.Items).Select(d => d.SourceId).ToList();
        Assert.Equal(250, allSourceIds.Distinct().Count());
    }
}
