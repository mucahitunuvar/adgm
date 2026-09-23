using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

// Unlike BuildingBlocks.TestDoubles.FakeFileStorageService (which always returns the same fixed
// FileAttachment), MediaAsset upload makes several UploadAsync calls per request (the original plus
// one per variant) that need distinct FileKeys - a real MediaAsset never has its Original and every
// Variant pointing at the same file.
public sealed class FakeMediaFileStorageService : IFileStorageService
{
    private int _uploadCount;

    public List<string> UploadedFileNames { get; } = [];

    public List<string> DeletedFileKeys { get; } = [];

    public bool ThrowOnDelete { get; set; }

    public Task<Result<FileAttachment>> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        FileCategory category,
        string ownerEntityType,
        Guid ownerEntityId,
        FileValidationPolicy validationPolicy,
        CancellationToken cancellationToken = default)
    {
        _uploadCount++;
        UploadedFileNames.Add(fileName);

        var attachment = FileAttachment.Create(
            $"website-images/2026/09/23/fake-{_uploadCount}.bin", fileName, contentType, content.Length,
            DateTime.UtcNow, ownerEntityType, ownerEntityId);

        return Task.FromResult(Result.Success(attachment));
    }

    public Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        if (ThrowOnDelete)
        {
            throw new IOException("Simulated file deletion failure.");
        }

        DeletedFileKeys.Add(fileKey);
        return Task.CompletedTask;
    }

    public Task<string> GetUrlAsync(string fileKey, CancellationToken cancellationToken = default) =>
        Task.FromResult($"http://localhost/webuploads/{fileKey}");

    public Task<byte[]?> ReadAsync(string fileKey, CancellationToken cancellationToken = default) =>
        Task.FromResult<byte[]?>(null);
}
