using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.ReferenceData;
using GenclikMerkezi.Modules.ReferenceData.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TaxOffice = GenclikMerkezi.Modules.ReferenceData.Domain.TaxOffice;

namespace GenclikMerkezi.Modules.ReferenceData.Features.AdminTaxOffice;

public sealed class UpdateTaxOfficeCommandHandler(
    IAdminLookupCrudService<TaxOffice> crudService,
    ICacheService cacheService,
    [FromKeyedServices(ReferenceDataModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateTaxOfficeCommand, Result>
{
    public async Task<Result> Handle(UpdateTaxOfficeCommand request, CancellationToken cancellationToken)
    {
        var entity = await crudService.FindByIdAsync(request.Id, cancellationToken);

        if (entity is null)
        {
            return Result.Failure(Error.NotFound("Lookup.NotFound", "The specified TaxOffice could not be found."));
        }

        entity.Update(request.DisplayName, request.SortOrder);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LookupCacheInvalidator.Invalidate(cacheService, ReferenceDataLookupType.TaxOffice);

        return Result.Success();
    }
}
