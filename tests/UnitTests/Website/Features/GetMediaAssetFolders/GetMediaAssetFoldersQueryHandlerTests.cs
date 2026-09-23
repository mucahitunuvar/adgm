using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Features.GetMediaAssetFolders;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.UnitTests.Website.TestDoubles;

namespace GenclikMerkezi.UnitTests.Website.Features.GetMediaAssetFolders;

public class GetMediaAssetFoldersQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsDistinctNonEmptyFolders()
    {
        var repository = new FakeMediaAssetRepository();
        var file = FileAttachment.Create(
            "website-images/2026/09/23/abc.jpg", "logo.jpg", "image/jpeg", 1024, DateTime.UtcNow, "MediaAsset", Guid.NewGuid());

        repository.Seed(MediaAsset.Create(
            Guid.NewGuid(), MediaAssetKind.Image, file, [], 800, 600, MediaFolder.Create("logos").Value,
            null, null, false, Guid.NewGuid(), DateTime.UtcNow).Value);
        repository.Seed(MediaAsset.Create(
            Guid.NewGuid(), MediaAssetKind.Image, file, [], 800, 600, MediaFolder.Create("logos").Value,
            null, null, false, Guid.NewGuid(), DateTime.UtcNow).Value);
        repository.Seed(MediaAsset.Create(
            Guid.NewGuid(), MediaAssetKind.Image, file, [], 800, 600, MediaFolder.Create("").Value,
            null, null, false, Guid.NewGuid(), DateTime.UtcNow).Value);

        var handler = new GetMediaAssetFoldersQueryHandler(repository);
        var result = await handler.Handle(new GetMediaAssetFoldersQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(["logos"], result.Value.Folders);
    }
}
