using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.DeleteMediaAsset;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace GenclikMerkezi.UnitTests.Website.Features.DeleteMediaAsset;

public class DeleteMediaAssetCommandHandlerTests
{
    private readonly FakeMediaAssetRepository _repository = new();
    private readonly FakeMediaUsageChecker _usageChecker = new();
    private readonly FakeMediaFileStorageService _fileStorageService = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private DeleteMediaAssetCommandHandler CreateHandler() =>
        new(_repository, _usageChecker, _fileStorageService, _unitOfWork, NullLogger<DeleteMediaAssetCommandHandler>.Instance);

    private static MediaAsset SeedMediaAssetWithVariant(FakeMediaAssetRepository repository)
    {
        var original = FileAttachment.Create(
            "website-images/2026/09/23/abc.jpg", "logo.jpg", "image/jpeg", 1024, DateTime.UtcNow, "MediaAsset", Guid.NewGuid());
        var variantFile = FileAttachment.Create(
            "website-images/2026/09/23/abc-small.webp", "logo-small.webp", "image/webp", 512, DateTime.UtcNow, "MediaAsset", Guid.NewGuid());
        var mediaAsset = MediaAsset.Create(
            Guid.NewGuid(), MediaAssetKind.Image, original, [MediaAssetVariant.Create("small", variantFile, 400, 300)],
            800, 600, MediaFolder.Create("logos").Value, null, null, false, Guid.NewGuid(), DateTime.UtcNow).Value;
        repository.Seed(mediaAsset);
        return mediaAsset;
    }

    [Fact]
    public async Task Handle_WithNoUsages_RemovesFromRepository_AndDeletesOriginalAndVariantFiles()
    {
        var mediaAsset = SeedMediaAssetWithVariant(_repository);

        var result = await CreateHandler().Handle(new DeleteMediaAssetCommand(mediaAsset.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(_repository.MediaAssets);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
        Assert.Contains("website-images/2026/09/23/abc.jpg", _fileStorageService.DeletedFileKeys);
        Assert.Contains("website-images/2026/09/23/abc-small.webp", _fileStorageService.DeletedFileKeys);
    }

    [Fact]
    public async Task Handle_WithExistingUsages_ReturnsConflict_DoesNotRemoveOrDeleteFiles()
    {
        var mediaAsset = SeedMediaAssetWithVariant(_repository);
        _usageChecker.UsagesToReturn = [new MediaUsage("content-item", Guid.NewGuid(), "Ana Sayfa - Hero", null)];

        var result = await CreateHandler().Handle(new DeleteMediaAssetCommand(mediaAsset.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("MediaAsset.InUse", result.Error.Code);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Single(_repository.MediaAssets);
        Assert.Empty(_fileStorageService.DeletedFileKeys);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenFileDeletionThrows_StillSucceeds()
    {
        var mediaAsset = SeedMediaAssetWithVariant(_repository);
        _fileStorageService.ThrowOnDelete = true;

        var result = await CreateHandler().Handle(new DeleteMediaAssetCommand(mediaAsset.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(_repository.MediaAssets);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new DeleteMediaAssetCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
