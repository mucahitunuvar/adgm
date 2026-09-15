using System.Reflection;
using FluentValidation;
using GenclikMerkezi.BuildingBlocks.Infrastructure.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.BuildingBlocks.Infrastructure.DependencyInjection;

public static class SharedInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSharedApplicationServices(
        this IServiceCollection services,
        params Assembly[] moduleAssemblies)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(moduleAssemblies);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssemblies(moduleAssemblies);

        return services;
    }
}
