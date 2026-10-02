using GenclikMerkezi.Modules.Website.Application.ImpactMetrics;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Application.ImpactMetrics;

public class ImpactMetricPublicQueryServiceTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LanguageCode En = LanguageCode.Create("en").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeImpactMetricRepository _impactMetricRepository = new();

    private ImpactMetricPublicQueryService CreateService() => new(_impactMetricRepository);

    private ImpactMetric SeedMetric(bool isActive = true, LanguageCode? translationLanguage = null, int sortOrder = 1)
    {
        var metric = ImpactMetric.Create(
            1000, "users", sortOrder, translationLanguage ?? Tr, "Desteklenen Genç", "genç", "2026 1. yarı", "Merkez iç kayıtları",
            UserId, Now).Value;
        if (isActive)
        {
            metric.Activate(translationLanguage ?? Tr, UserId, Now);
        }

        _impactMetricRepository.Seed(metric);
        return metric;
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsActiveMetricsOrderedBySortOrder()
    {
        SeedMetric(sortOrder: 2);
        SeedMetric(sortOrder: 1);

        var result = await CreateService().GetActiveAsync(Tr);

        Assert.Equal(2, result.Count);
        Assert.True(result[0].SortOrder <= result[1].SortOrder);
    }

    [Fact]
    public async Task GetActiveAsync_ExcludesInactiveMetrics()
    {
        SeedMetric(isActive: false);

        var result = await CreateService().GetActiveAsync(Tr);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetActiveAsync_ExcludesMetricsWithoutTranslationInRequestedLanguage()
    {
        SeedMetric(translationLanguage: Tr);

        var result = await CreateService().GetActiveAsync(En);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetActiveAsync_IncludesValueUnitPeriodAndSource()
    {
        SeedMetric();

        var result = await CreateService().GetActiveAsync(Tr);

        var metric = Assert.Single(result);
        Assert.Equal(1000, metric.Value);
        Assert.Equal("genç", metric.Unit);
        Assert.Equal("2026 1. yarı", metric.Period);
        Assert.Equal("Merkez iç kayıtları", metric.Source);
    }
}
