using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class NotFoundLogTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly DateTime Now = new(2026, 9, 28, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_WithValidPath_Succeeds()
    {
        var result = NotFoundLog.Create(Tr, "eski-sayfa", Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("eski-sayfa", result.Value.Path);
        Assert.Equal(1, result.Value.HitCount);
        Assert.Equal(Now, result.Value.FirstSeenAtUtc);
        Assert.Equal(Now, result.Value.LastSeenAtUtc);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyPath_Fails(string? path)
    {
        var result = NotFoundLog.Create(Tr, path, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("NotFoundLog.PathInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_WithPathLongerThanMaxLength_Fails()
    {
        var tooLong = new string('a', NotFoundLog.MaxPathLength + 1);

        var result = NotFoundLog.Create(Tr, tooLong, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("NotFoundLog.PathInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_WithPathAtMaxLength_Succeeds()
    {
        var maxLength = new string('a', NotFoundLog.MaxPathLength);

        var result = NotFoundLog.Create(Tr, maxLength, Now);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void RecordHit_IncrementsHitCountAndUpdatesLastSeenAtUtc()
    {
        var log = NotFoundLog.Create(Tr, "eski-sayfa", Now).Value;

        log.RecordHit(Now.AddDays(1));
        log.RecordHit(Now.AddDays(2));

        Assert.Equal(3, log.HitCount);
        Assert.Equal(Now, log.FirstSeenAtUtc);
        Assert.Equal(Now.AddDays(2), log.LastSeenAtUtc);
    }
}
