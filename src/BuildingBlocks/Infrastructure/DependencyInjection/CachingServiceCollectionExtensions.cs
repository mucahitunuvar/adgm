using GenclikMerkezi.BuildingBlocks.Infrastructure.Caching;
using GenclikMerkezi.BuildingBlocks.Infrastructure.Security;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.BuildingBlocks.Infrastructure.DependencyInjection;

public static class CachingServiceCollectionExtensions
{
    // Registered once at the Host composition root (ADR-017), mirroring AddMessaging<TDbContext>'s
    // single-registration pattern - every module gets ICacheService/IUserScopedCacheService through
    // its existing reference to this project, none registers its own.
    public static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CacheSettings>(configuration.GetSection(CacheSettings.SectionName));
        services.AddMemoryCache();

        // Singleton: IMemoryCache itself is already a singleton, and MemoryCacheService's own key
        // tracking set needs to live for the process, not per-request/per-scope.
        services.AddSingleton<ICacheService, MemoryCacheService>();

        // AddHttpContextAccessor() is idempotent (TryAddSingleton internally) - safe to call again
        // even though Identity's own module registration already calls it.
        services.AddHttpContextAccessor();
        services.AddSingleton<ICurrentUserContext, HttpContextCurrentUserContext>();
        services.AddScoped<IUserScopedCacheService, UserScopedCacheService>();

        return services;
    }
}
