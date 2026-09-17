using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.SharedKernel.Abstractions;

// Generic file storage contract (ADR-019). Implemented against the local disk today
// (LocalDiskFileStorageService, BuildingBlocks.Infrastructure); if an S3-compatible object store
// becomes necessary later, only that implementation changes - Domain/Application code depends only
// on this interface, never on a storage provider's SDK.
public interface IFileStorageService
{
    // Validates content against validationPolicy before writing anything, so a rejected upload
    // never touches storage. category namespaces the underlying storage location (ADR-019's
    // {category}/{yyyy}/{MM}/{dd}/{guid}.{ext} folder structure - see FileCategoryExtensions for the
    // category-to-folder-segment mapping); an out-of-range category value fails validation rather
    // than creating a stray folder. ownerEntityType/ownerEntityId become part of the returned
    // FileAttachment so the caller's aggregate can persist an auditable owner tag.
    Task<Result<FileAttachment>> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        FileCategory category,
        string ownerEntityType,
        Guid ownerEntityId,
        FileValidationPolicy validationPolicy,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default);

    Task<string> GetUrlAsync(string fileKey, CancellationToken cancellationToken = default);
}
