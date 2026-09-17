using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.ReferenceData.Features.GetLookupItems;

public sealed class GetLookupItemsQueryHandler(IReferenceDataLookupReader lookupReader)
    : IRequestHandler<GetLookupItemsQuery, Result<PagedResult<LookupItemSummary>>>
{
    public async Task<Result<PagedResult<LookupItemSummary>>> Handle(
        GetLookupItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await lookupReader.ListAsync(request.Type, request, request.ActiveOnly, cancellationToken);

        return Result.Success(items);
    }
}
