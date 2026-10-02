using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetImpactMetricById;

public sealed class GetImpactMetricByIdQueryHandler(IImpactMetricRepository impactMetricRepository)
    : IRequestHandler<GetImpactMetricByIdQuery, Result<ImpactMetricDetailResponse>>
{
    public async Task<Result<ImpactMetricDetailResponse>> Handle(GetImpactMetricByIdQuery request, CancellationToken cancellationToken)
    {
        var metric = await impactMetricRepository.GetByIdAsync(request.Id, cancellationToken);
        if (metric is null)
        {
            return Result.Failure<ImpactMetricDetailResponse>(
                Error.NotFound("ImpactMetric.NotFound", $"Impact metric '{request.Id}' could not be found."));
        }

        var translations = metric.Translations
            .Select(t => new ImpactMetricTranslationResponse(t.LanguageCode.Value, t.Label, t.Unit, t.Period, t.Source))
            .ToList();

        var response = new ImpactMetricDetailResponse(
            metric.Id, metric.Value, metric.IconKey, metric.SortOrder, metric.IsActive, metric.RowVersion, translations, metric.CreatedAtUtc);

        return Result.Success(response);
    }
}
