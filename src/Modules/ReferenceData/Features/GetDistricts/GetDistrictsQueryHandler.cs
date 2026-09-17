using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.ReferenceData.Features.GetDistricts;

public sealed class GetDistrictsQueryHandler(IReferenceDataLookupReader lookupReader)
    : IRequestHandler<GetDistrictsQuery, Result<PagedResult<LookupItemSummary>>>
{
    public async Task<Result<PagedResult<LookupItemSummary>>> Handle(
        GetDistrictsQuery request, CancellationToken cancellationToken)
    {
        var items = request.ProvinceId is { } provinceId
            ? await lookupReader.ListByParentAsync(
                ReferenceDataLookupType.District, provinceId, request, request.ActiveOnly, cancellationToken)
            : await lookupReader.ListAsync(
                ReferenceDataLookupType.District, request, request.ActiveOnly, cancellationToken);

        return Result.Success(items);
    }
}
