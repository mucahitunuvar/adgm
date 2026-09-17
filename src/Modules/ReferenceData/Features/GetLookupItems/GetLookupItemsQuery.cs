using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.ReferenceData.Features.GetLookupItems;

// Not generic over TLookup: IReferenceDataLookupReader is already keyed by the
// ReferenceDataLookupType enum, so one handler serves every lookup type that does not need its own
// parent-scoped filter (District is the one exception - GetDistrictsQuery) - there is nothing
// type-specific left to do on the read side.
public sealed record GetLookupItemsQuery(ReferenceDataLookupType Type, bool ActiveOnly)
    : PagedRequest, IRequest<Result<PagedResult<LookupItemSummary>>>;
