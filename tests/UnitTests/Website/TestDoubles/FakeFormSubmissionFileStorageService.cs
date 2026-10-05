using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

// AnonymizeExpiredFormSubmissionsJobTests needs per-key delete failure control (one file fails, another
// succeeds, the failed one is retried on the job's next run) - FakeMediaFileStorageService's single
// ThrowOnDelete flag can't express that.
public sealed class FakeFormSubmissionFileStorageService : IFileStorageService
{
    private readonly HashSet<string> _failDeleteForKeys = [];

    public List<string> DeletedFileKeys { get; } = [];

    public void FailDeleteFor(string fileKey) => _failDeleteForKeys.Add(fileKey);

    public Task<Result<FileAttachment>> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        FileCategory category,
        string ownerEntityType,
        Guid ownerEntityId,
        FileValidationPolicy validationPolicy,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not needed by anonymization job tests.");

    public Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        if (_failDeleteForKeys.Contains(fileKey))
        {
            throw new IOException($"Simulated failure deleting '{fileKey}'.");
        }

        DeletedFileKeys.Add(fileKey);
        return Task.CompletedTask;
    }

    public Task<string> GetUrlAsync(string fileKey, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not needed by anonymization job tests.");

    public Task<byte[]?> ReadAsync(string fileKey, CancellationToken cancellationToken = default) =>
        Task.FromResult<byte[]?>(null);
}
