using System.Text;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.UploadMediaAsset;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.UploadMediaAsset;

public class UploadMediaAssetCommandHandlerTests
{
    private readonly FakeMediaAssetRepository _mediaAssetRepository = new();
    private readonly FakeSiteLanguageRepository _siteLanguageRepository = new();
    private readonly FakeImageProcessor _imageProcessor = new();
    private readonly FakeMediaFileStorageService _fileStorageService = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private UploadMediaAssetCommandHandler CreateHandler() =>
        new(_mediaAssetRepository, _siteLanguageRepository, _imageProcessor, _fileStorageService,
            new FakeCurrentUserContext(Guid.NewGuid()), _unitOfWork);

    private static MemoryStream CreateContent(string text = "content") => new(Encoding.UTF8.GetBytes(text));

    [Fact]
    public async Task Handle_WithImage_CreatesMediaAssetWithOriginalAndVariants()
    {
        _imageProcessor.ResultToReturn = Result.Success(new ProcessedImage(
            [1, 2, 3], 800, 600,
            [
                new ProcessedImageVariant("small", [1], 400, 300),
                new ProcessedImageVariant("medium", [2], 800, 600),
            ]));

        var result = await CreateHandler().Handle(
            new UploadMediaAssetCommand(CreateContent(), "logo.png", "image/png", "logos", "Logo alt text", "Bir açıklama"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(800, result.Value.Width);
        Assert.Equal(600, result.Value.Height);
        Assert.Equal(2, result.Value.Variants.Count);

        var mediaAsset = Assert.Single(_mediaAssetRepository.MediaAssets);
        Assert.Equal(MediaAssetKind.Image, mediaAsset.Kind);
        Assert.Equal(2, mediaAsset.Variants.Count);
        Assert.Equal("logos", mediaAsset.Folder.Value);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);

        // Original + 2 variants = 3 uploads.
        Assert.Equal(3, _fileStorageService.UploadedFileNames.Count);
    }

    [Fact]
    public async Task Handle_WithAltTextAndDefaultLanguageSeeded_SetsTranslation()
    {
        var defaultLanguage = SiteLanguage.Create(LanguageCode.Create("tr").Value, "Türkçe", 1, Guid.NewGuid(), DateTime.UtcNow);
        defaultLanguage.MarkAsDefault(Guid.NewGuid(), DateTime.UtcNow);
        _siteLanguageRepository.Seed(defaultLanguage);

        var result = await CreateHandler().Handle(
            new UploadMediaAssetCommand(CreateContent(), "logo.png", "image/png", null, "Alt metin", "Açıklama"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var mediaAsset = Assert.Single(_mediaAssetRepository.MediaAssets);
        var translation = Assert.Single(mediaAsset.Translations);
        Assert.Equal("tr", translation.LanguageCode.Value);
        Assert.Equal("Alt metin", translation.AltText);
    }

    [Fact]
    public async Task Handle_WithUnsupportedExtension_ReturnsFailure_UploadsNothing()
    {
        var result = await CreateHandler().Handle(
            new UploadMediaAssetCommand(CreateContent(), "malware.exe", "application/octet-stream", null, null, null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("MediaAsset.UnsupportedFileType", result.Error.Code);
        Assert.Empty(_fileStorageService.UploadedFileNames);
        Assert.Empty(_mediaAssetRepository.MediaAssets);
    }

    [Fact]
    public async Task Handle_WhenImageProcessorFails_ReturnsFailure_UploadsNothing()
    {
        _imageProcessor.ResultToReturn = Result.Failure<ProcessedImage>(
            Error.Validation("MediaAsset.CannotDecodeImage", "not a real image"));

        var result = await CreateHandler().Handle(
            new UploadMediaAssetCommand(CreateContent(), "fake.png", "image/png", null, null, null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("MediaAsset.CannotDecodeImage", result.Error.Code);
        Assert.Empty(_fileStorageService.UploadedFileNames);
    }

    [Fact]
    public async Task Handle_WithPdfMissingRealSignature_ReturnsFailure()
    {
        var result = await CreateHandler().Handle(
            new UploadMediaAssetCommand(
                CreateContent("this is not really a pdf"), "document.pdf", "application/pdf", null, null, null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("MediaAsset.InvalidFileSignature", result.Error.Code);
        Assert.Empty(_fileStorageService.UploadedFileNames);
    }

    [Fact]
    public async Task Handle_WithValidPdfSignature_CreatesDocumentMediaAsset_WithNoVariants()
    {
        var pdfBytes = "%PDF-1.4\n...fake pdf content..."u8.ToArray();

        var result = await CreateHandler().Handle(
            new UploadMediaAssetCommand(new MemoryStream(pdfBytes), "document.pdf", "application/pdf", null, null, null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Width);
        Assert.Empty(result.Value.Variants);

        var mediaAsset = Assert.Single(_mediaAssetRepository.MediaAssets);
        Assert.Equal(MediaAssetKind.Document, mediaAsset.Kind);
        Assert.Empty(mediaAsset.Variants);
        Assert.Single(_fileStorageService.UploadedFileNames);
    }

    [Fact]
    public async Task Handle_WithInvalidFolder_ReturnsFailure()
    {
        var result = await CreateHandler().Handle(
            new UploadMediaAssetCommand(CreateContent(), "logo.png", "image/png", "nested/folder", null, null),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("MediaFolder.MustBeFlat", result.Error.Code);
    }
}
