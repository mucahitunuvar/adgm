using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.GetMediaAssets;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.GetMediaAssets;

public class GetMediaAssetsQueryHandlerTests
{
    private readonly FakeMediaAssetRepository _repository = new();
    private readonly FakeMediaFileStorageService _fileStorageService = new();

    private GetMediaAssetsQueryHandler CreateHandler() => new(_repository, _fileStorageService);

    private static MediaAsset CreateAsset(MediaAssetKind kind, string folder, string fileName, bool withAltText)
    {
        var file = FileAttachment.Create(
            "website-images/2026/09/23/abc.jpg", fileName, "image/jpeg", 1024, DateTime.UtcNow, "MediaAsset", Guid.NewGuid());
        var mediaAsset = MediaAsset.Create(
            Guid.NewGuid(), kind, file, [], 800, 600, MediaFolder.Create(folder).Value, null, null, false,
            Guid.NewGuid(), DateTime.UtcNow).Value;

        if (withAltText)
        {
            mediaAsset.SetTranslation(LanguageCode.Create("tr").Value, "Bir alt metin", null, Guid.NewGuid(), DateTime.UtcNow);
        }

        return mediaAsset;
    }

    [Fact]
    public async Task Handle_FiltersByKindFolderAndMissingAltText()
    {
        _repository.Seed(CreateAsset(MediaAssetKind.Image, "logos", "logo.jpg", withAltText: true));
        _repository.Seed(CreateAsset(MediaAssetKind.Image, "logos", "undocumented.jpg", withAltText: false));
        _repository.Seed(CreateAsset(MediaAssetKind.Document, "documents", "brochure.pdf", withAltText: false));

        var result = await CreateHandler().Handle(
            new GetMediaAssetsQuery("Image", "logos", null, MissingAltText: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal("undocumented.jpg", item.OriginalFileName);
        Assert.False(item.HasAltText);
    }

    [Fact]
    public async Task Handle_WithInvalidKind_ReturnsFailure()
    {
        var result = await CreateHandler().Handle(new GetMediaAssetsQuery("NotAKind", null, null, null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }
}
