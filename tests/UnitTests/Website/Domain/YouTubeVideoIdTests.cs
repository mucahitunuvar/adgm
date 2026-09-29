using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class YouTubeVideoIdTests
{
    private const string Id = "dQw4w9WgXcQ";

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("http://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://m.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch?list=PL123&v=dQw4w9WgXcQ&t=10s")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ")]
    [InlineData("https://www.youtu.be/dQw4w9WgXcQ")]
    [InlineData("https://youtube.com/embed/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ")]
    [InlineData("https://youtube.com/shorts/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/shorts/dQw4w9WgXcQ")]
    [InlineData("https://youtube-nocookie.com/embed/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ")]
    public void Create_WithRecognizedUrlFormat_ExtractsVideoId(string url)
    {
        var result = YouTubeVideoId.Create(url);

        Assert.True(result.IsSuccess);
        Assert.Equal(Id, result.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-url")]
    [InlineData("https://vimeo.com/123456789")]
    [InlineData("https://www.youtube.com/watch")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXc")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQQ")]
    [InlineData("https://youtu.be/dQw4w9WgXc")]
    [InlineData("https://youtu.be/dQw4w9WgXcQQ")]
    [InlineData("ftp://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("javascript:alert(1)")]
    [InlineData("https://www.youtube.com/playlist?list=PL123")]
    public void Create_WithUnrecognizedOrInvalidUrl_Fails(string? url)
    {
        var result = YouTubeVideoId.Create(url);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void BuildEmbedUrl_UsesYouTubeNocookieDomain()
    {
        var videoId = YouTubeVideoId.Create("https://youtu.be/dQw4w9WgXcQ").Value;

        Assert.Equal("https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ", videoId.BuildEmbedUrl());
    }

    [Fact]
    public void BuildThumbnailUrl_UsesYtimgDomain()
    {
        var videoId = YouTubeVideoId.Create("https://youtu.be/dQw4w9WgXcQ").Value;

        Assert.Equal("https://i.ytimg.com/vi/dQw4w9WgXcQ/hqdefault.jpg", videoId.BuildThumbnailUrl());
    }
}
