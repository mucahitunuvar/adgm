using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class LinkTargetTests
{
    [Fact]
    public void CreateEmpty_IsEmpty()
    {
        var target = LinkTarget.CreateEmpty();

        Assert.True(target.IsEmpty);
        Assert.Equal(LinkTargetKind.None, target.Kind);
    }

    [Fact]
    public void ForContent_WithEmptyGuid_Fails()
    {
        var result = LinkTarget.ForContent(Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.Equal("LinkTarget.ContentItemIdRequired", result.Error.Code);
    }

    [Fact]
    public void ForContent_WithValidGuid_Succeeds()
    {
        var id = Guid.NewGuid();
        var result = LinkTarget.ForContent(id);

        Assert.True(result.IsSuccess);
        Assert.Equal(LinkTargetKind.Content, result.Value.Kind);
        Assert.Equal(id, result.Value.ContentItemId);
        Assert.False(result.Value.IsEmpty);
    }

    [Fact]
    public void ForContentTypeListing_WithEmptyGuid_Fails()
    {
        var result = LinkTarget.ForContentTypeListing(Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.Equal("LinkTarget.ContentTypeIdRequired", result.Error.Code);
    }

    [Theory]
    [InlineData("/portal/giris")]
    [InlineData("/a")]
    public void ForInternalPath_WithValidPath_Succeeds(string path)
    {
        var result = LinkTarget.ForInternalPath(path);

        Assert.True(result.IsSuccess);
        Assert.Equal(LinkTargetKind.InternalPath, result.Value.Kind);
        Assert.Equal(path, result.Value.InternalPath);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("portal/giris")]
    [InlineData("//portal/giris")]
    [InlineData("/portal//giris")]
    [InlineData(@"/portal\giris")]
    [InlineData("/portal:giris")]
    [InlineData("https://example.com")]
    public void ForInternalPath_WithInvalidPath_Fails(string? path)
    {
        var result = LinkTarget.ForInternalPath(path);

        Assert.True(result.IsFailure);
        Assert.Equal("LinkTarget.InternalPathInvalid", result.Error.Code);
    }

    [Fact]
    public void ForInternalPath_LongerThanMaxLength_Fails()
    {
        var path = "/" + new string('a', LinkTarget.MaxInternalPathLength);

        var result = LinkTarget.ForInternalPath(path);

        Assert.True(result.IsFailure);
        Assert.Equal("LinkTarget.InternalPathInvalid", result.Error.Code);
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://example.com/path")]
    [InlineData("mailto:info@example.com")]
    [InlineData("tel:+901234567890")]
    public void ForExternalUrl_WithAllowedScheme_Succeeds(string url)
    {
        var result = LinkTarget.ForExternalUrl(url);

        Assert.True(result.IsSuccess);
        Assert.Equal(LinkTargetKind.ExternalUrl, result.Value.Kind);
        Assert.Equal(url, result.Value.ExternalUrl);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ftp://example.com")]
    [InlineData("javascript:alert(1)")]
    [InlineData("example.com")]
    [InlineData("/relative/path")]
    public void ForExternalUrl_WithDisallowedSchemeOrRelative_Fails(string? url)
    {
        var result = LinkTarget.ForExternalUrl(url);

        Assert.True(result.IsFailure);
        Assert.Equal("LinkTarget.ExternalUrlInvalid", result.Error.Code);
    }

    [Fact]
    public void Equality_IsValueBased()
    {
        var id = Guid.NewGuid();
        var first = LinkTarget.ForContent(id).Value;
        var second = LinkTarget.ForContent(id).Value;

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }
}
