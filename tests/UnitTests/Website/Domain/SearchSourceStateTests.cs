using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class SearchSourceStateTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_WithValidSourceKey_Succeeds()
    {
        var result = SearchSourceState.Create("employer.job");

        Assert.True(result.IsSuccess);
        Assert.Equal("employer.job", result.Value.SourceKey);
        Assert.Null(result.Value.LastStartedAtUtc);
        Assert.Null(result.Value.LastSucceededAtUtc);
        Assert.Equal(0, result.Value.DocumentCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithMissingSourceKey_Fails(string sourceKey)
    {
        var result = SearchSourceState.Create(sourceKey);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_WithSourceKeyLongerThanMax_Fails()
    {
        var result = SearchSourceState.Create(new string('a', SearchSourceState.MaxSourceKeyLength + 1));

        Assert.True(result.IsFailure);
        Assert.Equal("SearchSourceState.SourceKeyInvalid", result.Error.Code);
    }

    [Fact]
    public void MarkStarted_SetsLastStartedAtUtc()
    {
        var state = SearchSourceState.Create("website").Value;

        state.MarkStarted(Now);

        Assert.Equal(Now, state.LastStartedAtUtc);
    }

    [Fact]
    public void MarkSucceeded_SetsSucceededTimeAndCount_AndClearsAnyPreviousError()
    {
        var state = SearchSourceState.Create("website").Value;
        state.MarkFailed("boom");

        state.MarkSucceeded(Now, 42);

        Assert.Equal(Now, state.LastSucceededAtUtc);
        Assert.Equal(42, state.DocumentCount);
        Assert.Null(state.LastError);
    }

    [Fact]
    public void MarkFailed_WithShortMessage_StoresItAsIs()
    {
        var state = SearchSourceState.Create("website").Value;

        state.MarkFailed("connection timed out");

        Assert.Equal("connection timed out", state.LastError);
    }

    [Fact]
    public void MarkFailed_WithMessageLongerThanMax_Truncates_NoStackTrace()
    {
        var state = SearchSourceState.Create("website").Value;
        var longMessage = new string('x', SearchSourceState.MaxLastErrorLength + 100);

        state.MarkFailed(longMessage);

        Assert.Equal(SearchSourceState.MaxLastErrorLength, state.LastError!.Length);
    }
}
