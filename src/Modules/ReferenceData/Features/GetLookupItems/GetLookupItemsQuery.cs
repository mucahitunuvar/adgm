using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.ReferenceData.Features.GetLookupItems;

// Not generic over TLookup: IReferenceDataLookupReader is already keyed by the
// ReferenceDataLookupType enum, so one handler serves all 23 lookup types (SEED and
// ADMIN-MANAGED alike) - there is nothing type-specific left to do on the read side.
public sealed record GetLookupItemsQuery(ReferenceDataLookupType Type, bool ActiveOnly)
    : IRequest<Result<IReadOnlyList<LookupItemSummary>>>;
