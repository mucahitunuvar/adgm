using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetImpactMetrics;

public sealed record GetImpactMetricsQuery(bool? IsActive, string? Search)
    : PagedRequest, IRequest<Result<PagedResult<ImpactMetricSummaryResponse>>>;
