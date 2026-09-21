using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;

// IFileStorageService SharedKernel'de tanımlı (ADR-019), FakeCurrentUserContext/FakeTimeProvider ile
// aynı gerekçeyle burada, modüller-arası paylaşılan bir konumda yaşıyor.
public sealed class FakeFileStorageService : IFileStorageService
{
    public Result<FileAttachment> UploadResult { get; set; } = Result.Success(FileAttachment.Create(
        "employer-logos/2026/09/21/fake.png", "fake.png", "image/png", 1024, DateTime.UtcNow, "Company", Guid.NewGuid()));

    public string UrlToReturn { get; set; } = "http://localhost/uploads/employer-logos/2026/09/21/fake.png";

    public List<string> DeletedFileKeys { get; } = [];

    public (Stream Content, string FileName, string ContentType, FileCategory Category, string OwnerEntityType, Guid OwnerEntityId)? UploadCall
    { get; private set; }

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
        UploadCall = (content, fileName, contentType, category, ownerEntityType, ownerEntityId);
        return Task.FromResult(UploadResult);
    }

    public Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        DeletedFileKeys.Add(fileKey);
        return Task.CompletedTask;
    }

    public Task<string> GetUrlAsync(string fileKey, CancellationToken cancellationToken = default) =>
        Task.FromResult(UrlToReturn);

    public Task<byte[]?> ReadAsync(string fileKey, CancellationToken cancellationToken = default) =>
        Task.FromResult<byte[]?>(null);
}
