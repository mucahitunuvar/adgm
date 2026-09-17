using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.ReferenceData.Features.GetLookupItems;

public sealed class GetLookupItemsQueryHandler(IReferenceDataLookupReader lookupReader)
    : IRequestHandler<GetLookupItemsQuery, Result<IReadOnlyList<LookupItemSummary>>>
{
    public async Task<Result<IReadOnlyList<LookupItemSummary>>> Handle(
        GetLookupItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await lookupReader.ListAsync(request.Type, request.ActiveOnly, cancellationToken);

        return Result.Success(items);
    }
}
