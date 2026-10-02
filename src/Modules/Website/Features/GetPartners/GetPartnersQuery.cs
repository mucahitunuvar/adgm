using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPartners;

public sealed record GetPartnersQuery(bool? IsActive, string? Search) : PagedRequest, IRequest<Result<PagedResult<PartnerSummaryResponse>>>;
