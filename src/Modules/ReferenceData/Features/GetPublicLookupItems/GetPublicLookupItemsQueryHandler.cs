using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.ReferenceData.Features.GetPublicLookupItems;

public sealed class GetPublicLookupItemsQueryHandler(IReferenceDataLookupReader lookupReader)
    : IRequestHandler<GetPublicLookupItemsQuery, Result<PagedResult<PublicLookupItemResponse>>>
{
    public async Task<Result<PagedResult<PublicLookupItemResponse>>> Handle(
        GetPublicLookupItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await lookupReader.ListAsync(request.Type, request, activeOnly: true, cancellationToken);
        var mapped = items.Items.Select(i => new PublicLookupItemResponse(i.Id, i.DisplayName)).ToList();

        return Result.Success(new PagedResult<PublicLookupItemResponse>(mapped, items.TotalCount, items.Page, items.PageSize));
    }
}
