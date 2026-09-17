using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.ReferenceData.Application.Abstractions;
using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.DependencyInjection;

public static class ReferenceDataModuleServiceCollectionExtensions
{
    public static IServiceCollection AddReferenceDataModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ReferenceDataDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("ReferenceDataDatabase");

            // ADR-012's usual Sqlite-for-Testing switch applies here (unlike Identity):
            // ReferenceData never publishes messages, so it is never the CAP transactional outbox
            // anchor and has no constraint tying it to SqlServer in every environment.
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
            ReferenceDataModuleMarker.UnitOfWorkKey,
            (sp, _) => sp.GetRequiredService<ReferenceDataDbContext>());

        // ICacheService (which ReferenceDataLookupReader depends on) is registered centrally by
        // AddCaching at the Host composition root (ADR-017) - no per-module cache registration here.
        services.AddScoped<IReferenceDataLookupReader, ReferenceDataLookupReader>();

        // Open generic registration: resolves IAdminLookupCrudService<TLookup> for any TLookup at
        // the generic command handlers' call sites (ADR-016 Decision 3) without registering one
        // line per lookup type. This works because IAdminLookupCrudService<TLookup> is a plain
        // single-parameter open generic - .NET's container maps it directly.
        services.AddScoped(typeof(IAdminLookupCrudService<>), typeof(AdminLookupCrudService<>));

        // The generic Create/Update/Deactivate MediatR handlers (ADR-016 Decision 3) are NOT
        // auto-registered by AddMediatR's assembly scan (Program.cs's AddSharedApplicationServices):
        // scanning only finds CLOSED generic handler types, and CreateLookupItemCommandHandler<T> is
        // never closed anywhere in this module's own code - only at each lookup type's call site.
        // C# also does not allow expressing "IRequestHandler<CreateLookupItemCommand<>, Result<Guid>>"
        // (a partially-open nested generic) as a typeof() target (CS7003), and .NET's container's
        // open-generic support only maps a fully-open service type to a fully-open implementation,
        // not this "one free parameter nested inside a closed one" shape. So each admin-managed
        // lookup type registers its own closed handlers/validators here, one call each - the same
        // "one call per type" cost LookupEndpoints.MapWithCrud<TLookup> already has for routes.
        services.AddLookupCrudHandlers<Sector>();
        services.AddLookupCrudHandlers<Position>();
        services.AddLookupCrudHandlers<Department>();
        services.AddLookupCrudHandlers<WorkLocationType>();
        services.AddLookupCrudHandlers<EmploymentType>();
        services.AddLookupCrudHandlers<EducationLevel>();
        services.AddLookupCrudHandlers<Gender>();
        services.AddLookupCrudHandlers<MilitaryStatus>();
        services.AddLookupCrudHandlers<DriversLicenseType>();
        services.AddLookupCrudHandlers<LanguageLevel>();
        services.AddLookupCrudHandlers<ExperienceLevel>();
        services.AddLookupCrudHandlers<Nationality>();
        services.AddLookupCrudHandlers<DisabilityCategory>();
        services.AddLookupCrudHandlers<DiplomaGradingSystem>();
        services.AddLookupCrudHandlers<ReferenceType>();
        services.AddLookupCrudHandlers<Currency>();
        services.AddLookupCrudHandlers<Skill>();
        services.AddLookupCrudHandlers<SchoolCategory>();
        services.AddLookupCrudHandlers<WorkField>();
        services.AddLookupCrudHandlers<School>();

        return services;
    }
}
