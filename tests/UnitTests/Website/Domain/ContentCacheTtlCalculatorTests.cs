using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class ContentCacheTtlCalculatorTests
{
    private static readonly DateTime Now = new(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Calculate_WithNoUpcomingTransitions_ReturnsDefaultTtl()
    {
        var ttl = ContentCacheTtlCalculator.Calculate(Now, [null, null]);

        Assert.Equal(ContentCacheTtlCalculator.DefaultTtl, ttl);
    }

    [Fact]
    public void Calculate_WithOnlyPastTransitions_ReturnsDefaultTtl()
    {
        var ttl = ContentCacheTtlCalculator.Calculate(Now, [Now.AddMinutes(-5)]);

        Assert.Equal(ContentCacheTtlCalculator.DefaultTtl, ttl);
    }

    [Fact]
    public void Calculate_WithTransitionSoonerThanDefaultTtl_ReturnsTimeUntilTransition()
    {
        var ttl = ContentCacheTtlCalculator.Calculate(Now, [Now.AddMinutes(3)]);

        Assert.Equal(TimeSpan.FromMinutes(3), ttl);
    }

    [Fact]
    public void Calculate_WithTransitionLaterThanDefaultTtl_ReturnsDefaultTtl()
    {
        var ttl = ContentCacheTtlCalculator.Calculate(Now, [Now.AddHours(1)]);

        Assert.Equal(ContentCacheTtlCalculator.DefaultTtl, ttl);
    }

    [Fact]
    public void Calculate_WithMultipleTransitions_UsesTheEarliestFutureOne()
    {
        var ttl = ContentCacheTtlCalculator.Calculate(Now, [Now.AddMinutes(-1), Now.AddMinutes(7), Now.AddMinutes(2)]);

        Assert.Equal(TimeSpan.FromMinutes(2), ttl);
    }

    [Fact]
    public void Calculate_WithTransitionRightNow_ReturnsAtLeastOneSecond()
    {
        var ttl = ContentCacheTtlCalculator.Calculate(Now, [Now.AddMilliseconds(1)]);

        Assert.True(ttl >= TimeSpan.FromSeconds(1));
    }
}
