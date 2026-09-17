using FluentValidation;
using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.Modules.ReferenceData.Features.AdminLookupCrud;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.ReferenceData.Infrastructure.DependencyInjection;

// Registers the closed forms of ADR-016 Decision 3's generic Create/Update/Deactivate handlers and
// validators for one admin-managed lookup type - see ReferenceDataModuleServiceCollectionExtensions
// for why this cannot be a single open-generic registration.
internal static class LookupCrudServiceCollectionExtensions
{
    public static IServiceCollection AddLookupCrudHandlers<TLookup>(this IServiceCollection services)
        where TLookup : LookupItem, ILookupItemFactory<TLookup>
    {
        services.AddScoped<IRequestHandler<CreateLookupItemCommand<TLookup>, Result<Guid>>, CreateLookupItemCommandHandler<TLookup>>();
        services.AddScoped<IRequestHandler<UpdateLookupItemCommand<TLookup>, Result>, UpdateLookupItemCommandHandler<TLookup>>();
        services.AddScoped<IRequestHandler<DeactivateLookupItemCommand<TLookup>, Result>, DeactivateLookupItemCommandHandler<TLookup>>();

        services.AddScoped<IValidator<CreateLookupItemCommand<TLookup>>, CreateLookupItemCommandValidator<TLookup>>();
        services.AddScoped<IValidator<UpdateLookupItemCommand<TLookup>>, UpdateLookupItemCommandValidator<TLookup>>();

        return services;
    }
}
