using GenclikMerkezi.Modules.ReferenceData;
using GenclikMerkezi.Modules.ReferenceData.Application.Abstractions;
using GenclikMerkezi.Modules.ReferenceData.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.ReferenceData.Features.AdminLookupCrud;

public sealed class CreateLookupItemCommandHandler<TLookup>(
    IAdminLookupCrudService<TLookup> crudService,
    IMemoryCache cache,
    [FromKeyedServices(ReferenceDataModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateLookupItemCommand<TLookup>, Result<Guid>>
    where TLookup : LookupItem, ILookupItemFactory<TLookup>
{
    public async Task<Result<Guid>> Handle(CreateLookupItemCommand<TLookup> request, CancellationToken cancellationToken)
    {
        var codeExists = await crudService.CodeExistsAsync(request.Code, cancellationToken);

        if (codeExists)
        {
            return Result.Failure<Guid>(
                Error.Conflict("Lookup.CodeAlreadyExists", $"A {typeof(TLookup).Name} with this code already exists."));
        }

        var entity = TLookup.Create(request.Code, request.DisplayName, request.SortOrder);
        crudService.Add(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LookupCacheInvalidator.Invalidate(cache, TLookup.LookupType);

        return Result.Success(entity.Id);
    }
}
