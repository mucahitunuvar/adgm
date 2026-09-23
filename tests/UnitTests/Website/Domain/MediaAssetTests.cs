using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class MediaAssetTests
{
    private static FileAttachment CreateFile(string key = "website-images/2026/09/23/abc.jpg") =>
        FileAttachment.Create(key, "photo.jpg", "image/jpeg", 1024, DateTime.UtcNow, "MediaAsset", Guid.NewGuid());

    private static Result<MediaAsset> CreateImage(bool containsPersonalData = false, string? usagePermissionNote = null) =>
        MediaAsset.Create(
            Guid.NewGuid(), MediaAssetKind.Image, CreateFile(), [], 800, 600,
            MediaFolder.Create("logos").Value, "Unsplash", usagePermissionNote, containsPersonalData,
            Guid.NewGuid(), DateTime.UtcNow);

    [Fact]
    public void Create_WithoutPersonalData_Succeeds()
    {
        var result = CreateImage();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithPersonalDataButNoUsagePermissionNote_Fails()
    {
        var result = CreateImage(containsPersonalData: true, usagePermissionNote: null);

        Assert.True(result.IsFailure);
        Assert.Equal("MediaAsset.UsagePermissionNoteRequired", result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    [Fact]
    public void Create_WithPersonalDataAndUsagePermissionNote_Succeeds()
    {
        var result = CreateImage(containsPersonalData: true, usagePermissionNote: "Written consent on file.");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void UpdateMetadata_TurningOnPersonalDataWithoutNote_Fails_AndLeavesStateUnchanged()
    {
        var mediaAsset = CreateImage().Value;

        var result = mediaAsset.UpdateMetadata(
            MediaFolder.Create("logos").Value, "Unsplash", null, containsPersonalData: true,
            Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal("MediaAsset.UsagePermissionNoteRequired", result.Error.Code);
        Assert.False(mediaAsset.ContainsPersonalData);
    }

    [Fact]
    public void SetTranslation_ForNewLanguage_Adds()
    {
        var mediaAsset = CreateImage().Value;
        var tr = LanguageCode.Create("tr").Value;

        mediaAsset.SetTranslation(tr, "Logo görseli", "Açıklama", Guid.NewGuid(), DateTime.UtcNow);

        var translation = Assert.Single(mediaAsset.Translations);
        Assert.Equal(tr, translation.LanguageCode);
        Assert.Equal("Logo görseli", translation.AltText);
        Assert.Equal("Açıklama", translation.Caption);
    }

    [Fact]
    public void SetTranslation_ForExistingLanguage_Updates_DoesNotDuplicate()
    {
        var mediaAsset = CreateImage().Value;
        var tr = LanguageCode.Create("tr").Value;
        mediaAsset.SetTranslation(tr, "İlk", "İlk açıklama", Guid.NewGuid(), DateTime.UtcNow);

        mediaAsset.SetTranslation(tr, "Güncel", "Güncel açıklama", Guid.NewGuid(), DateTime.UtcNow);

        var translation = Assert.Single(mediaAsset.Translations);
        Assert.Equal("Güncel", translation.AltText);
        Assert.Equal("Güncel açıklama", translation.Caption);
    }
}
