using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.Extensions.Configuration;

namespace GenclikMerkezi.Api.Website;

// Host-level adapter (ADR-024 §10, Faz 5 Görev 4): implements Website's IExternalSearchSource port by
// pulling published job summaries from Employer's public contract (IPublishedJobModuleContract, ADR-023
// §6). Neither module knows about the other - only Host (the composition root) is allowed to reference
// both GenclikMerkezi.Contracts.Employer and Website's own Application abstractions, the same
// NotificationWebsiteEmailSender-style wiring just above this file.
//
// Jobs are published in a single language (no per-language content), so they are indexed in the site's
// default language only - they simply never surface in a search for any other language (ADR-024 §10
// assumption, documented since IExternalSearchSource/ExternalSearchDocument has no LanguageCode of its
// own; the default-language resolution lives in Website's ExternalSearchSourceSynchronizer, not here).
public sealed class EmployerJobSearchSource(
    IPublishedJobModuleContract publishedJobModuleContract, ISiteSettingsRepository siteSettingsRepository, IConfiguration configuration)
    : IExternalSearchSource
{
    public string SourceKey => "employer.job";

    public async Task<PagedResult<ExternalSearchDocument>> GetPublishedDocumentsAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var settings = await siteSettingsRepository.GetAsync(cancellationToken) ?? SiteSettings.CreateDefault();
        if (!settings.PublicJobListingsEnabled)
        {
            // ADR-024 §10: an empty page here, every page, is how a full sync ends up deleting
            // everything this source previously indexed - jobs drop out of search/sitemap entirely.
            return new PagedResult<ExternalSearchDocument>([], 0, page, pageSize);
        }

        var jobsPage = await publishedJobModuleContract.GetPublishedJobSummariesAsync(page, pageSize, cancellationToken);
        var pathPrefix = (configuration["Website:JobsPublicPathPrefix"] ?? "/ilanlar").TrimEnd('/');

        var items = jobsPage.Items
            .Select(job => new ExternalSearchDocument(
                job.JobId.ToString(),
                "job",
                job.Title,
                job.Summary,
                $"{pathPrefix}/{job.Slug}",
                BuildSearchableText(job),
                job.PublishedAtUtc,
                IncludeInSitemap: true))
            .ToList();

        return new PagedResult<ExternalSearchDocument>(items, jobsPage.TotalCount, jobsPage.Page, jobsPage.PageSize);
    }

    private static string BuildSearchableText(PublishedJobSummary job) =>
        string.Join(
            ' ',
            new[] { job.Title, job.CompanyName, job.ProvinceName, job.PositionName, job.EmploymentTypeName, job.WorkLocationTypeName, job.Summary }
                .Where(part => !string.IsNullOrWhiteSpace(part)));
}
