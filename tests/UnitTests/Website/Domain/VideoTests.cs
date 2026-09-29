using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class VideoTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LanguageCode En = LanguageCode.Create("en").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime Now = new(2026, 9, 29, 12, 0, 0, DateTimeKind.Utc);
    private const string Url = "https://youtu.be/dQw4w9WgXcQ";

    [Fact]
    public void Create_WithValidInput_Succeeds()
    {
        var result = Video.Create(Url, null, 1, Tr, "Tanıtım Videosu", null, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("dQw4w9WgXcQ", result.Value.YouTubeVideoId.Value);
        Assert.True(result.Value.IsActive);
        Assert.Single(result.Value.Translations);
        Assert.Equal("Tanıtım Videosu", result.Value.Translations[0].Title);
    }

    [Fact]
    public void Create_WithInvalidUrl_Fails()
    {
        var result = Video.Create("https://vimeo.com/123", null, 1, Tr, "Başlık", null, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("YouTubeVideoId.Invalid", result.Error.Code);
    }

    [Fact]
    public void Create_WithEmptyDefaultLanguageTitle_Fails()
    {
        var result = Video.Create(Url, null, 1, Tr, "  ", null, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("VideoTranslation.TitleInvalid", result.Error.Code);
    }

    [Fact]
    public void Update_WithValidInput_ChangesUrlCoverAndSortOrder()
    {
        var video = Video.Create(Url, null, 1, Tr, "Başlık", null, UserId, Now).Value;
        var coverImageId = Guid.NewGuid();

        var result = video.Update("https://youtube.com/embed/M7lc1UVf-VE", coverImageId, 5, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("M7lc1UVf-VE", video.YouTubeVideoId.Value);
        Assert.Equal(coverImageId, video.CoverImageMediaId);
        Assert.Equal(5, video.SortOrder);
        Assert.NotNull(video.UpdatedAtUtc);
    }

    [Fact]
    public void Update_WithInvalidUrl_Fails()
    {
        var video = Video.Create(Url, null, 1, Tr, "Başlık", null, UserId, Now).Value;

        var result = video.Update("not-a-url", null, 1, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("YouTubeVideoId.Invalid", result.Error.Code);
    }

    [Fact]
    public void SetTranslation_ForNewLanguage_AddsTranslation()
    {
        var video = Video.Create(Url, null, 1, Tr, "Başlık", null, UserId, Now).Value;

        var result = video.SetTranslation(En, "Title", "Description", UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, video.Translations.Count);
    }

    [Fact]
    public void SetTranslation_ForExistingLanguage_UpdatesInPlace()
    {
        var video = Video.Create(Url, null, 1, Tr, "Başlık", null, UserId, Now).Value;

        var result = video.SetTranslation(Tr, "Yeni Başlık", "Açıklama", UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Single(video.Translations);
        Assert.Equal("Yeni Başlık", video.Translations[0].Title);
        Assert.Equal("Açıklama", video.Translations[0].Description);
    }

    [Fact]
    public void RemoveTranslation_ForDefaultLanguage_Fails()
    {
        var video = Video.Create(Url, null, 1, Tr, "Başlık", null, UserId, Now).Value;

        var result = video.RemoveTranslation(Tr, Tr, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Video.CannotDeleteDefaultTranslation", result.Error.Code);
    }

    [Fact]
    public void RemoveTranslation_ForNonDefaultLanguage_Succeeds()
    {
        var video = Video.Create(Url, null, 1, Tr, "Başlık", null, UserId, Now).Value;
        video.SetTranslation(En, "Title", null, UserId, Now);

        var result = video.RemoveTranslation(En, Tr, UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.Single(video.Translations);
    }

    [Fact]
    public void RemoveTranslation_WhenMissing_Fails()
    {
        var video = Video.Create(Url, null, 1, Tr, "Başlık", null, UserId, Now).Value;

        var result = video.RemoveTranslation(En, Tr, UserId, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("Video.TranslationNotFound", result.Error.Code);
    }

    [Fact]
    public void Activate_SetsIsActiveTrue()
    {
        var video = Video.Create(Url, null, 1, Tr, "Başlık", null, UserId, Now).Value;
        video.Deactivate(UserId, Now);

        var result = video.Activate(UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.True(video.IsActive);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var video = Video.Create(Url, null, 1, Tr, "Başlık", null, UserId, Now).Value;

        var result = video.Deactivate(UserId, Now);

        Assert.True(result.IsSuccess);
        Assert.False(video.IsActive);
    }

    [Fact]
    public void Touch_RegeneratesRowVersion()
    {
        var video = Video.Create(Url, null, 1, Tr, "Başlık", null, UserId, Now).Value;
        var originalRowVersion = video.RowVersion;

        video.Update(Url, null, 2, UserId, Now);

        Assert.NotEqual(originalRowVersion, video.RowVersion);
    }
}
