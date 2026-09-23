using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.UpdateMediaAsset;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.UpdateMediaAsset;

public class UpdateMediaAssetCommandHandlerTests
{
    private readonly FakeMediaAssetRepository _repository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private UpdateMediaAssetCommandHandler CreateHandler() =>
        new(_repository, new FakeCurrentUserContext(Guid.NewGuid()), _unitOfWork);

    private static MediaAsset SeedMediaAsset(FakeMediaAssetRepository repository)
    {
        var file = FileAttachment.Create(
            "website-images/2026/09/23/abc.jpg", "logo.jpg", "image/jpeg", 1024, DateTime.UtcNow, "MediaAsset", Guid.NewGuid());
        var mediaAsset = MediaAsset.Create(
            Guid.NewGuid(), MediaAssetKind.Image, file, [], 800, 600,
            MediaFolder.Create("logos").Value, null, null, false, Guid.NewGuid(), DateTime.UtcNow).Value;
        repository.Seed(mediaAsset);
        return mediaAsset;
    }

    [Fact]
    public async Task Handle_UpdatesFolderSourceAndPermissionNote()
    {
        var mediaAsset = SeedMediaAsset(_repository);

        var result = await CreateHandler().Handle(
            new UpdateMediaAssetCommand(mediaAsset.Id, "partners", "Unsplash", null, false, null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("partners", mediaAsset.Folder.Value);
        Assert.Equal("Unsplash", mediaAsset.Source);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_TurningOnPersonalDataWithoutNote_ReturnsFailure_DoesNotSave()
    {
        var mediaAsset = SeedMediaAsset(_repository);

        var result = await CreateHandler().Handle(
            new UpdateMediaAssetCommand(mediaAsset.Id, "logos", null, null, true, null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("MediaAsset.UsagePermissionNoteRequired", result.Error.Code);
        Assert.False(mediaAsset.ContainsPersonalData);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithTranslations_UpsertsEachOne()
    {
        var mediaAsset = SeedMediaAsset(_repository);
        var translations = new[]
        {
            new UpdateMediaAssetTranslationInput("tr", "Logo", "Açıklama"),
            new UpdateMediaAssetTranslationInput("en", "Logo", "Description"),
        };

        var result = await CreateHandler().Handle(
            new UpdateMediaAssetCommand(mediaAsset.Id, "logos", null, null, false, translations),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, mediaAsset.Translations.Count);
        Assert.Contains(mediaAsset.Translations, t => t.LanguageCode.Value == "tr" && t.AltText == "Logo");
        Assert.Contains(mediaAsset.Translations, t => t.LanguageCode.Value == "en" && t.Caption == "Description");
    }

    [Fact]
    public async Task Handle_WithInvalidLanguageCode_ReturnsFailure_DoesNotSave()
    {
        var mediaAsset = SeedMediaAsset(_repository);
        var translations = new[] { new UpdateMediaAssetTranslationInput("123", "Logo", null) };

        var result = await CreateHandler().Handle(
            new UpdateMediaAssetCommand(mediaAsset.Id, "logos", null, null, false, translations),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("LanguageCode.InvalidFormat", result.Error.Code);
        Assert.Empty(mediaAsset.Translations);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(
            new UpdateMediaAssetCommand(Guid.NewGuid(), null, null, null, false, null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
