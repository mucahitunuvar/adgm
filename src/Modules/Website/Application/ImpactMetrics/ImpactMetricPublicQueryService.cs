using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.ImpactMetrics;

// ADR-024 §8.2 (Faz 2 Görev 3): "Public: ayrı endpoint yok; Görev 5'te blok verisi olarak döner.
// Sorgu servisi bu görevde yazılır" - the single Application-layer service the impact-stats block
// (Görev 5) will call, the same "service now, consumer later" shape SliderPublicQueryService already
// established in Görev 2.
public sealed class ImpactMetricPublicQueryService(IImpactMetricRepository impactMetricRepository)
{
    public async Task<IReadOnlyList<PublicImpactMetricResponse>> GetActiveAsync(
        LanguageCode languageCode, CancellationToken cancellationToken = default)
    {
        var metrics = await impactMetricRepository.SearchActiveAsync(languageCode, cancellationToken);

        return metrics
            .Select(metric =>
            {
                // Guaranteed to exist: SearchActiveAsync only returns metrics with a translation in
                // languageCode (§1 "çeviri kuralı").
                var translation = metric.Translations.First(t => t.LanguageCode == languageCode);
                return new PublicImpactMetricResponse(
                    metric.Id, metric.Value, translation.Unit, translation.Label, translation.Period, translation.Source,
                    metric.IconKey, metric.SortOrder);
            })
            .ToList();
    }
}
