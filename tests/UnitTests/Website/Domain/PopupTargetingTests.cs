using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class PopupTargetingTests
{
    [Fact]
    public void CreateForContents_WithValidIds_Succeeds()
    {
        var result = PopupTargeting.CreateForContents([Guid.NewGuid(), Guid.NewGuid()]);

        Assert.True(result.IsSuccess);
        Assert.Equal(PopupTargetingKind.Contents, result.Value.Kind);
    }

    [Fact]
    public void CreateForContents_Empty_Fails()
    {
        var result = PopupTargeting.CreateForContents([]);

        Assert.True(result.IsFailure);
        Assert.Equal("PopupTargeting.ContentsRequired", result.Error.Code);
    }

    [Fact]
    public void CreateForContents_MoreThanFifty_Fails()
    {
        var ids = Enumerable.Range(0, 51).Select(_ => Guid.NewGuid()).ToList();

        var result = PopupTargeting.CreateForContents(ids);

        Assert.True(result.IsFailure);
        Assert.Equal("PopupTargeting.TooManyContents", result.Error.Code);
    }

    [Fact]
    public void CreateForPaths_WithValidPaths_Succeeds()
    {
        var result = PopupTargeting.CreateForPaths(["/haberler", "/haberler/*"]);

        Assert.True(result.IsSuccess);
        Assert.Equal(PopupTargetingKind.Paths, result.Value.Kind);
    }

    [Fact]
    public void CreateForPaths_Empty_Fails()
    {
        var result = PopupTargeting.CreateForPaths([]);

        Assert.True(result.IsFailure);
        Assert.Equal("PopupTargeting.PathsRequired", result.Error.Code);
    }

    [Theory]
    [InlineData("haberler")] // missing leading slash
    [InlineData("/haberler//alt")] // double slash
    [InlineData("/hab:erler")] // scheme-like colon
    [InlineData("/hab\\erler")] // backslash
    public void CreateForPaths_InvalidFormat_Fails(string path)
    {
        var result = PopupTargeting.CreateForPaths([path]);

        Assert.True(result.IsFailure);
        Assert.Equal("PopupTargeting.PathInvalid", result.Error.Code);
    }

    [Theory]
    [InlineData("/haberler/*/alt")] // '*' not at the end
    [InlineData("/haberler*")] // '*' not preceded by '/'
    public void CreateForPaths_WildcardNotAtEnd_Fails(string path)
    {
        var result = PopupTargeting.CreateForPaths([path]);

        Assert.True(result.IsFailure);
        Assert.Equal("PopupTargeting.PathInvalid", result.Error.Code);
    }

    [Fact]
    public void CreateForPaths_MoreThanTwenty_Fails()
    {
        var paths = Enumerable.Range(0, 21).Select(i => $"/path-{i}").ToList();

        var result = PopupTargeting.CreateForPaths(paths);

        Assert.True(result.IsFailure);
        Assert.Equal("PopupTargeting.TooManyPaths", result.Error.Code);
    }

    [Fact]
    public void CreateForPaths_DuplicatePath_Fails()
    {
        var result = PopupTargeting.CreateForPaths(["/haberler", "/haberler"]);

        Assert.True(result.IsFailure);
        Assert.Equal("PopupTargeting.DuplicatePath", result.Error.Code);
    }

    [Fact]
    public void CreateAllPages_And_CreateHomeOnly_CarryNoLists()
    {
        var allPages = PopupTargeting.CreateAllPages();
        var homeOnly = PopupTargeting.CreateHomeOnly();

        Assert.Empty(allPages.ContentItemIds);
        Assert.Empty(allPages.Paths);
        Assert.Empty(homeOnly.ContentItemIds);
        Assert.Empty(homeOnly.Paths);
    }
}
