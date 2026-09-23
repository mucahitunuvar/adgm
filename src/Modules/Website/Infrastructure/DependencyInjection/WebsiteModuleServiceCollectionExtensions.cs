using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Infrastructure;
using GenclikMerkezi.Modules.Website.Infrastructure.Persistence;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Infrastructure.DependencyInjection;

public static class WebsiteModuleServiceCollectionExtensions
{
    // ADR-024 §2 / AGENTS.md §1: literal "Admin" role string, same as every other module (ADR-016
    // pattern) - never a reference to Identity.Domain.UserRole, which Website must not depend on.
    private const string AdminRole = "Admin";

    public static IServiceCollection AddWebsiteModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<WebsiteDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("WebsiteDatabase");

            // ADR-012: Sqlite-for-Testing switch, same as every other module's AddXModule().
            if (string.Equals(configuration["Database:Provider"], "Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                options.UseSqlServer(connectionString);
            }
        });

        services.AddKeyedScoped<IUnitOfWork>(
            WebsiteModuleMarker.UnitOfWorkKey,
            (sp, _) => sp.GetRequiredService<WebsiteDbContext>());

        services.AddScoped<ISiteLanguageRepository, SiteLanguageRepository>();

        // ADR-024 §2: named policies, all resolving to the literal Admin role for now. Only this
        // block changes when a real permission system arrives - endpoints stay untouched.
        services.AddAuthorization(options =>
        {
            options.AddPolicy(WebsitePolicies.ContentManage, policy => policy.RequireRole(AdminRole));
            options.AddPolicy(WebsitePolicies.ContentPublish, policy => policy.RequireRole(AdminRole));
            options.AddPolicy(WebsitePolicies.StructureManage, policy => policy.RequireRole(AdminRole));
            options.AddPolicy(WebsitePolicies.DesignManage, policy => policy.RequireRole(AdminRole));
            options.AddPolicy(WebsitePolicies.SettingsManage, policy => policy.RequireRole(AdminRole));
            options.AddPolicy(WebsitePolicies.SubmissionsView, policy => policy.RequireRole(AdminRole));
            options.AddPolicy(WebsitePolicies.SubmissionsManage, policy => policy.RequireRole(AdminRole));
        });

        return services;
    }
}
