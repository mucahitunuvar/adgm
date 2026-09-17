using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.ReferenceData.Features.GetDistricts;

// District is the one SEED lookup with a practical need to filter by its parent (975 rows across
// 81 provinces) - a bespoke query/endpoint instead of the generic GetLookupItemsQuery/
// LookupEndpoints.MapReadOnly, mirroring the AdminTaxOffice feature's bespoke pattern for the same
// reason (ADR-016 Decision 3 only makes the plain, filter-less shape generic).
public sealed record GetDistrictsQuery(Guid? ProvinceId, bool ActiveOnly)
    : PagedRequest, IRequest<Result<PagedResult<LookupItemSummary>>>;
