using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.GetMediaAssetById;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.GetMediaAssetById;

public class GetMediaAssetByIdQueryHandlerTests
{
    private readonly FakeMediaAssetRepository _repository = new();
    private readonly FakeMediaFileStorageService _fileStorageService = new();
    private readonly FakeMediaUsageChecker _usageChecker = new();

    private GetMediaAssetByIdQueryHandler CreateHandler() =>
        new(_repository, _fileStorageService, _usageChecker);

    [Fact]
    public async Task Handle_WithExistingAsset_ReturnsDetailIncludingUsages()
    {
        var file = FileAttachment.Create(
            "website-images/2026/09/23/abc.jpg", "logo.jpg", "image/jpeg", 1024, DateTime.UtcNow, "MediaAsset", Guid.NewGuid());
        var mediaAsset = MediaAsset.Create(
            Guid.NewGuid(), MediaAssetKind.Image, file, [], 800, 600,
            MediaFolder.Create("logos").Value, null, null, false, Guid.NewGuid(), DateTime.UtcNow).Value;
        _repository.Seed(mediaAsset);
        _usageChecker.UsagesToReturn = [new MediaUsage("site-settings", Guid.NewGuid(), "Site ayarları - Logo", "/admin/settings")];

        var result = await CreateHandler().Handle(new GetMediaAssetByIdQuery(mediaAsset.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("logo.jpg", result.Value.OriginalFileName);
        Assert.Equal(800, result.Value.Width);
        var usage = Assert.Single(result.Value.Usages);
        Assert.Equal("Site ayarları - Logo", usage.Description);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new GetMediaAssetByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }
}
