using GenclikMerkezi.BuildingBlocks.Infrastructure.FileStorage;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.BuildingBlocks.Infrastructure.DependencyInjection;

public static class FileStorageServiceCollectionExtensions
{
    // Registered once at the Host composition root (ADR-019), mirroring AddCaching's single-
    // registration pattern - every module gets IFileStorageService through its existing reference
    // to this project, none registers its own.
    public static IServiceCollection AddFileStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileStorageSettings>(configuration.GetSection(FileStorageSettings.SectionName));
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IFileStorageService, LocalDiskFileStorageService>();

        return services;
    }
}
