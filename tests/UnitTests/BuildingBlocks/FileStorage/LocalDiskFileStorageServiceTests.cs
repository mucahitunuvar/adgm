using System.Text;
using GenclikMerkezi.BuildingBlocks.Infrastructure.FileStorage;
using GenclikMerkezi.SharedKernel.Domain;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.BuildingBlocks.FileStorage;

public sealed class LocalDiskFileStorageServiceTests : IDisposable
{
    private readonly string _rootDirectory = Path.Combine(Path.GetTempPath(), $"gm-filestorage-tests-{Guid.NewGuid():N}");

    private LocalDiskFileStorageService CreateService(string publicBaseUrl = "http://localhost/uploads") =>
        new(Options.Create(new FileStorageSettings { RootDirectory = _rootDirectory, PublicBaseUrl = publicBaseUrl }));

    private static FileValidationPolicy CreatePhotoPolicy() =>
        FileValidationPolicy.Create(["jpg", "png"], ["image/jpeg", "image/png"], maxSizeInBytes: 2 * 1024 * 1024);

    private static MemoryStream CreateContent(int sizeInBytes) => new(new byte[sizeInBytes]);

    [Fact]
    public async Task UploadAsync_WithValidFile_WritesToDiskAndReturnsAttachment()
    {
        var service = CreateService();
        var ownerId = Guid.NewGuid();
        using var content = new MemoryStream(Encoding.UTF8.GetBytes("fake-image-bytes"));

        var result = await service.UploadAsync(
            content, "photo.jpg", "image/jpeg", "candidate-photos", "CandidateCv", ownerId, CreatePhotoPolicy());

        Assert.True(result.IsSuccess);
        var attachment = result.Value;
        Assert.Equal("photo.jpg", attachment.OriginalFileName);
        Assert.Equal("image/jpeg", attachment.ContentType);
        Assert.Equal("CandidateCv", attachment.OwnerEntityType);
        Assert.Equal(ownerId, attachment.OwnerEntityId);
        Assert.StartsWith("candidate-photos/", attachment.FileKey);
        Assert.EndsWith(".jpg", attachment.FileKey);

        var fullPath = Path.Combine(_rootDirectory, attachment.FileKey.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(fullPath));
    }

    [Fact]
    public async Task UploadAsync_WithDisallowedExtension_ReturnsFailure_AndDoesNotWriteToDisk()
    {
        var service = CreateService();
        using var content = CreateContent(1024);

        var result = await service.UploadAsync(
            content, "cv.pdf", "image/jpeg", "candidate-photos", "CandidateCv", Guid.NewGuid(), CreatePhotoPolicy());

        Assert.True(result.IsFailure);
        Assert.Equal("FileStorage.InvalidExtension", result.Error.Code);
        Assert.False(Directory.Exists(_rootDirectory));
    }

    [Fact]
    public async Task UploadAsync_WithSizeOverLimit_ReturnsFailure_AndDoesNotWriteToDisk()
    {
        var service = CreateService();
        using var content = CreateContent(3 * 1024 * 1024);

        var result = await service.UploadAsync(
            content, "photo.jpg", "image/jpeg", "candidate-photos", "CandidateCv", Guid.NewGuid(), CreatePhotoPolicy());

        Assert.True(result.IsFailure);
        Assert.Equal("FileStorage.FileTooLarge", result.Error.Code);
        Assert.False(Directory.Exists(_rootDirectory));
    }

    [Fact]
    public async Task DeleteAsync_RemovesUploadedFile()
    {
        var service = CreateService();
        using var content = new MemoryStream(Encoding.UTF8.GetBytes("fake-image-bytes"));
        var uploaded = await service.UploadAsync(
            content, "photo.jpg", "image/jpeg", "candidate-photos", "CandidateCv", Guid.NewGuid(), CreatePhotoPolicy());
        var fullPath = Path.Combine(_rootDirectory, uploaded.Value.FileKey.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(fullPath));

        await service.DeleteAsync(uploaded.Value.FileKey);

        Assert.False(File.Exists(fullPath));
    }

    [Fact]
    public async Task DeleteAsync_WithUnknownKey_DoesNotThrow()
    {
        var service = CreateService();

        var exception = await Record.ExceptionAsync(() => service.DeleteAsync("candidate-photos/does-not-exist.jpg"));

        Assert.Null(exception);
    }

    [Fact]
    public async Task GetUrlAsync_BuildsUrlFromConfiguredBaseUrlAndFileKey()
    {
        var service = CreateService("http://localhost:5289/uploads");

        var url = await service.GetUrlAsync("candidate-photos/abc123.jpg");

        Assert.Equal("http://localhost:5289/uploads/candidate-photos/abc123.jpg", url);
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootDirectory))
        {
            Directory.Delete(_rootDirectory, recursive: true);
        }
    }
}
