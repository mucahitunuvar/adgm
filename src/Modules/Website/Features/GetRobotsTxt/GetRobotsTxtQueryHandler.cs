using System.Text;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace GenclikMerkezi.Modules.Website.Features.GetRobotsTxt;

// ADR-024 §15 (Faz 5 Görev 5): GET /robots.txt (root path, anonymous). Deliberately uncached (§33: no
// demonstrated need - a single-row SiteSettings read plus a handful of configuration reads, not a
// query that fans out across the database like the sitemap's own content walk) so a maintenance-mode or
// AllowSearchEngineIndexing flip takes effect on the very next request.
public sealed class GetRobotsTxtQueryHandler(ISiteSettingsRepository siteSettingsRepository, IConfiguration configuration)
    : IRequestHandler<GetRobotsTxtQuery, Result<string>>
{
    private static readonly string[] DefaultDisallowedPaths = ["/api/", "/admin", "/portal", "/webuploads/private"];

    public async Task<Result<string>> Handle(GetRobotsTxtQuery request, CancellationToken cancellationToken)
    {
        var settings = await siteSettingsRepository.GetAsync(cancellationToken) ?? SiteSettings.CreateDefault();
        var builder = new StringBuilder();
        builder.Append("User-agent: *\n");

        // ADR-024 §15: both conditions together, not either alone - a maintenance window with indexing
        // otherwise allowed still lets crawlers see the normal disallow list (and the sitemap itself
        // returns the real content, maintenance mode never hides it from GetSitemapQueryHandler).
        if (settings.MaintenanceModeEnabled && !settings.AllowSearchEngineIndexing)
        {
            builder.Append("Disallow: /\n");
            return Result.Success(builder.ToString());
        }

        var disallowedPaths = configuration.GetSection("Website:Robots:DisallowedPaths").Get<string[]>() ?? DefaultDisallowedPaths;
        foreach (var path in disallowedPaths)
        {
            builder.Append($"Disallow: {path}\n");
        }

        var publicSiteBaseUrl = (configuration["Website:PublicSiteBaseUrl"] ?? string.Empty).TrimEnd('/');
        builder.Append('\n');
        builder.Append($"Sitemap: {publicSiteBaseUrl}/sitemap.xml\n");

        return Result.Success(builder.ToString());
    }
}
