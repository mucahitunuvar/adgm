using GenclikMerkezi.Modules.ReferenceData;
using GenclikMerkezi.Modules.ReferenceData.Application.Abstractions;
using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.ReferenceData.Features.AdminLookupCrud;

public sealed class DeactivateLookupItemCommandHandler<TLookup>(
    IAdminLookupCrudService<TLookup> crudService,
    IMemoryCache cache,
    [FromKeyedServices(ReferenceDataModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivateLookupItemCommand<TLookup>, Result>
    where TLookup : LookupItem, ILookupItemFactory<TLookup>
{
    public async Task<Result> Handle(DeactivateLookupItemCommand<TLookup> request, CancellationToken cancellationToken)
    {
        var entity = await crudService.FindByIdAsync(request.Id, cancellationToken);

        if (entity is null)
        {
            return Result.Failure(
                Error.NotFound("Lookup.NotFound", $"The specified {typeof(TLookup).Name} could not be found."));
        }

        entity.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LookupCacheInvalidator.Invalidate(cache, TLookup.LookupType);

        return Result.Success();
    }
}
