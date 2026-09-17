using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.Modules.ReferenceData;
using GenclikMerkezi.Modules.ReferenceData.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TaxOffice = GenclikMerkezi.Modules.ReferenceData.Domain.TaxOffice;

namespace GenclikMerkezi.Modules.ReferenceData.Features.AdminTaxOffice;

public sealed class CreateTaxOfficeCommandHandler(
    IAdminLookupCrudService<TaxOffice> crudService,
    IReferenceDataLookupReader lookupReader,
    ICacheService cacheService,
    [FromKeyedServices(ReferenceDataModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateTaxOfficeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateTaxOfficeCommand request, CancellationToken cancellationToken)
    {
        var provinceExists = await lookupReader.ExistsAndActiveAsync(
            ReferenceDataLookupType.Province, request.ProvinceId, cancellationToken);

        if (!provinceExists)
        {
            return Result.Failure<Guid>(
                Error.NotFound("TaxOffice.ProvinceNotFound", "The specified province could not be found."));
        }

        var codeExists = await crudService.CodeExistsAsync(request.Code, cancellationToken);

        if (codeExists)
        {
            return Result.Failure<Guid>(
                Error.Conflict("Lookup.CodeAlreadyExists", "A TaxOffice with this code already exists."));
        }

        var entity = TaxOffice.Create(request.Code, request.DisplayName, request.SortOrder, request.ProvinceId);
        crudService.Add(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LookupCacheInvalidator.Invalidate(cacheService, ReferenceDataLookupType.TaxOffice);

        return Result.Success(entity.Id);
    }
}
