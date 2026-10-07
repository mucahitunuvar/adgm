using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class SearchDocumentTests
{
    private static LanguageCode Tr() => LanguageCode.Create("tr").Value;

    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_WithValidFields_Succeeds()
    {
        var result = SearchDocument.Create(
            "website", "content-123", Tr(), "news", "Başlık", "Özet", "/haberler/baslik",
            "BASLIK OZET", Now, Now, true);

        Assert.True(result.IsSuccess);
        Assert.Equal("website", result.Value.SourceKey);
        Assert.Equal("content-123", result.Value.SourceId);
        Assert.Equal("news", result.Value.TypeKey);
        Assert.Equal("Başlık", result.Value.Title);
        Assert.True(result.Value.IncludeInSitemap);
    }

    [Theory]
    [InlineData("", "content-1")]
    [InlineData(" ", "content-1")]
    [InlineData("website", "")]
    [InlineData("website", " ")]
    public void Create_WithMissingSourceKeyOrSourceId_Fails(string sourceKey, string sourceId)
    {
        var result = SearchDocument.Create(
            sourceKey, sourceId, Tr(), "news", "Başlık", "Özet", "/url", "NORM", Now, Now, true);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_WithSourceKeyLongerThanMax_Fails()
    {
        var result = SearchDocument.Create(
            new string('a', SearchDocument.MaxSourceKeyLength + 1), "content-1", Tr(), "news",
            "Başlık", "Özet", "/url", "NORM", Now, Now, true);

        Assert.True(result.IsFailure);
        Assert.Equal("SearchDocument.SourceKeyInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_WithTitleLongerThanMax_Fails()
    {
        var result = SearchDocument.Create(
            "website", "content-1", Tr(), "news", new string('a', SearchDocument.MaxTitleLength + 1),
            "Özet", "/url", "NORM", Now, Now, true);

        Assert.True(result.IsFailure);
        Assert.Equal("SearchDocument.TitleInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_WithSummaryLongerThanMax_Fails()
    {
        var result = SearchDocument.Create(
            "website", "content-1", Tr(), "news", "Başlık", new string('a', SearchDocument.MaxSummaryLength + 1),
            "/url", "NORM", Now, Now, true);

        Assert.True(result.IsFailure);
        Assert.Equal("SearchDocument.SummaryTooLong", result.Error.Code);
    }

    [Fact]
    public void Create_WithUrlMissing_Fails()
    {
        var result = SearchDocument.Create(
            "website", "content-1", Tr(), "news", "Başlık", "Özet", "", "NORM", Now, Now, true);

        Assert.True(result.IsFailure);
        Assert.Equal("SearchDocument.UrlInvalid", result.Error.Code);
    }

    [Fact]
    public void Create_WithNormalizedTextLongerThanMax_Fails()
    {
        var result = SearchDocument.Create(
            "website", "content-1", Tr(), "news", "Başlık", "Özet", "/url",
            new string('a', SearchDocument.MaxNormalizedTextLength + 1), Now, Now, true);

        Assert.True(result.IsFailure);
        Assert.Equal("SearchDocument.NormalizedTextTooLong", result.Error.Code);
    }

    [Fact]
    public void Refresh_UpdatesContentFields_ButNeverTheNaturalKey()
    {
        var document = SearchDocument.Create(
            "website", "content-1", Tr(), "news", "Başlık", "Özet", "/url", "NORM", Now, Now, true).Value;

        var refreshResult = document.Refresh(
            "announcement", "Yeni Başlık", "Yeni Özet", "/yeni-url", "YENI NORM", Now.AddDays(1), Now.AddDays(1), false);

        Assert.True(refreshResult.IsSuccess);
        Assert.Equal("website", document.SourceKey);
        Assert.Equal("content-1", document.SourceId);
        Assert.Equal("announcement", document.TypeKey);
        Assert.Equal("Yeni Başlık", document.Title);
        Assert.False(document.IncludeInSitemap);
    }
}
