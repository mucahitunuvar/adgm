using GenclikMerkezi.Contracts.ReferenceData;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.ReferenceData.Features.GetPublicLookupItems;

// Unlike GetLookupItemsQuery (admin), ActiveOnly is not a caller-controlled parameter here: an
// anonymous dropdown must never be able to ask for deactivated options (madde 39).
public sealed record GetPublicLookupItemsQuery(ReferenceDataLookupType Type)
    : PagedRequest, IRequest<Result<PagedResult<PublicLookupItemResponse>>>;
