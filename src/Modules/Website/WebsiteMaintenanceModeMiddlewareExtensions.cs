using System.Globalization;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website;

// Faz 3 Koşu 1 (FRONTEND-ACIK-MADDELER.md madde "bakım modu zorlaması yok" bulgusu): until now,
// SiteSettings.MaintenanceModeEnabled was just a data field frontend had to enforce itself. This
// middleware is the actual enforcement: every /api/v1/public/* request gets 503 + Retry-After while
// the flag is on, EXCEPT /api/v1/public/site (the one public endpoint the frontend reads the flag
// from to render its own maintenance page) - auth/admin/health/openapi never match the path prefix at
// all, so they need no explicit exclusion. The flag is read through ICacheService under
// WebsiteCacheKeys.MaintenanceEnabled (same "public-site" prefix every SiteSettings mutation already
// invalidates via WebsiteCacheInvalidator.InvalidateAllPublic), so this never hits the database on the
// common case of an unchanged setting.
public static class WebsiteMaintenanceModeMiddlewareExtensions
{
    private const string PublicPrefix = "/api/v1/public";
    private const string PublicSiteExemptSuffix = "/site";
    private const int DefaultRetryAfterSeconds = 300;

    public static IApplicationBuilder UseWebsiteMaintenanceMode(this IApplicationBuilder app)
    {
        var retryAfterSeconds = app.ApplicationServices.GetRequiredService<IConfiguration>()
            .GetValue("Website:MaintenanceRetryAfterSeconds", DefaultRetryAfterSeconds)
            .ToString(CultureInfo.InvariantCulture);

        return app.Use(async (context, next) =>
        {
            if (!AppliesTo(context.Request.Path))
            {
                await next(context);
                return;
            }

            var cacheService = context.RequestServices.GetRequiredService<ICacheService>();
            var siteSettingsRepository = context.RequestServices.GetRequiredService<ISiteSettingsRepository>();

            var maintenanceEnabled = await cacheService.GetOrCreateAsync(
                WebsiteCacheKeys.MaintenanceEnabled,
                async cancellationToken =>
                {
                    var settings = await siteSettingsRepository.GetAsync(cancellationToken) ?? SiteSettings.CreateDefault();
                    return settings.MaintenanceModeEnabled;
                },
                cancellationToken: context.RequestAborted);

            if (!maintenanceEnabled)
            {
                await next(context);
                return;
            }

            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.Headers.RetryAfter = retryAfterSeconds;
        });
    }

    private static bool AppliesTo(PathString path) =>
        path.StartsWithSegments(PublicPrefix, out var remaining) &&
        !remaining.Equals(PublicSiteExemptSuffix, StringComparison.OrdinalIgnoreCase);
}
