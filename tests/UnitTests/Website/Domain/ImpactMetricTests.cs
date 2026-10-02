using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class ImpactMetricTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LanguageCode En = LanguageCode.Create("en").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc);

    private static ImpactMetric CreateMetric(string? period = "2026 1. yarı", string? source = "Merkez iç kayıtları") =>
        Create(period, source).Value;

    private static GenclikMerkezi.SharedKernel.Results.Result<ImpactMetric> Create(string? period, string? source) =>
        ImpactMetric.Create(1000, "users", 1, Tr, "Desteklenen Genç", "genç", period, source, UserId, Now);

    [Fact]
    public void Create_StartsInactive()
    {
        var metric = CreateMetric();

        Assert.False(metric.IsActive);
        Assert.Equal(1000, metric.Value);
        Assert.Single(metric.Translations);
        Assert.Equal("Desteklenen Genç", metric.Translations[0].Label);
    }

    [Fact]
    public void Create_WithNegativeValue_Fails()
    {
        var result = ImpactMetric.Create(-1, null, 1, Tr, "Label", null, null, null, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ImpactMetric.ValueInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_WithMoreThanTwoDecimalPlaces_Fails()
    {
        var result = ImpactMetric.Create(1.234m, null, 1, Tr, "Label", null, null, null, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ImpactMetric.ValueInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_WithInvalidIconKey_Fails()
    {
        var result = ImpactMetric.Create(1, "Invalid Key!", 1, Tr, "Label", null, null, null, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ImpactMetric.IconKeyInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_WithEmptyDefaultLanguageLabel_Fails()
    {
        var result = ImpactMetric.Create(1, null, 1, Tr, "  ", null, null, null, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ImpactMetricTranslation.LabelInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_WithoutPeriodOrSource_Succeeds_ButStaysInactive()
    {
        var result = Create(null, null);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsActive);
    }

    [Fact]
    public void Activate_WithoutDefaultLanguagePeriodOrSource_Fails()
    {
        var metric = Create(null, null).Value;

        var result = metric.Activate(Tr, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ImpactMetric.ActivationRequiresPeriodAndSource", result.Error.Code);
        Assert.False(metric.IsActive);
    }

    [Fact]
    public void Activate_WithDefaultLanguagePeriodAndSource_Succeeds()
    {
        var metric = CreateMetric();

        var result = metric.Activate(Tr, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.True(metric.IsActive);
    }

    [Fact]
    public void SetTranslation_WhileActive_CannotClearDefaultLanguagePeriod()
    {
        var metric = CreateMetric();
        metric.Activate(Tr, UserId, Now);

        var result = metric.SetTranslation(Tr, Tr, "Desteklenen Genç", "genç", null, "Merkez iç kayıtları", UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ImpactMetric.CannotClearPeriodOrSourceWhileActive", result.Error.Code);
    }

    [Fact]
    public void SetTranslation_WhileActive_CannotClearDefaultLanguageSource()
    {
        var metric = CreateMetric();
        metric.Activate(Tr, UserId, Now);

        var result = metric.SetTranslation(Tr, Tr, "Desteklenen Genç", "genç", "2026 1. yarı", null, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ImpactMetric.CannotClearPeriodOrSourceWhileActive", result.Error.Code);
    }

    [Fact]
    public void SetTranslation_ForNonDefaultLanguage_CanOmitPeriodAndSourceEvenWhileActive()
    {
        var metric = CreateMetric();
        metric.Activate(Tr, UserId, Now);

        var result = metric.SetTranslation(En, Tr, "Supported Youth", "youth", null, null, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, metric.Translations.Count);
    }

    [Fact]
    public void RemoveTranslation_ForDefaultLanguage_Fails()
    {
        var metric = CreateMetric();

        var result = metric.RemoveTranslation(Tr, Tr, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("ImpactMetric.CannotDeleteDefaultTranslation", result.Error.Code);
    }

    [Fact]
    public void RemoveTranslation_ForNonDefaultLanguage_Succeeds()
    {
        var metric = CreateMetric();
        metric.SetTranslation(En, Tr, "Supported Youth", null, null, null, UserId, Now);

        var result = metric.RemoveTranslation(En, Tr, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Single(metric.Translations);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var metric = CreateMetric();
        metric.Activate(Tr, UserId, Now);

        var result = metric.Deactivate(UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.False(metric.IsActive);
    }

    [Fact]
    public void Touch_RegeneratesRowVersion()
    {
        var metric = CreateMetric();
        var originalRowVersion = metric.RowVersion;

        metric.Update(2000, "users", 2, UserId, Now);

        Assert.NotEqual(originalRowVersion, metric.RowVersion);
    }
}
