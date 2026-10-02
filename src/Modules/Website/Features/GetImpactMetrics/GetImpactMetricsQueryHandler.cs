using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetImpactMetrics;

public sealed class GetImpactMetricsQueryHandler(IImpactMetricRepository impactMetricRepository, ISiteLanguageRepository siteLanguageRepository)
    : IRequestHandler<GetImpactMetricsQuery, Result<PagedResult<ImpactMetricSummaryResponse>>>
{
    public async Task<Result<PagedResult<ImpactMetricSummaryResponse>>> Handle(GetImpactMetricsQuery request, CancellationToken cancellationToken)
    {
        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure<PagedResult<ImpactMetricSummaryResponse>>(
                Error.Failure("ImpactMetric.NoDefaultLanguage", "No default site language is configured."));
        }

        var paged = await impactMetricRepository.SearchAsync(request.IsActive, request.Search, defaultLanguage.Code, request, cancellationToken);

        var items = paged.Items.Select(metric => ToSummary(metric, defaultLanguage.Code)).ToList();

        return Result.Success(new PagedResult<ImpactMetricSummaryResponse>(items, paged.TotalCount, paged.Page, paged.PageSize));
    }

    private static ImpactMetricSummaryResponse ToSummary(ImpactMetric metric, LanguageCode defaultLanguageCode)
    {
        var translation = metric.Translations.FirstOrDefault(t => t.LanguageCode == defaultLanguageCode);

        return new ImpactMetricSummaryResponse(
            metric.Id, metric.Value, translation?.Unit, translation?.Label ?? string.Empty, metric.IconKey, metric.SortOrder, metric.IsActive,
            metric.RowVersion);
    }
}
