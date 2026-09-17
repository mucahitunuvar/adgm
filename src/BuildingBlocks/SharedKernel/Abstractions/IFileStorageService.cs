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
    // never touches storage. folder namespaces the underlying storage location (e.g.
    // "candidate-photos", "candidate-cv-files"); ownerEntityType/ownerEntityId become part of the
    // returned FileAttachment so the caller's aggregate can persist an auditable owner tag.
    Task<Result<FileAttachment>> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        string folder,
        string ownerEntityType,
        Guid ownerEntityId,
        FileValidationPolicy validationPolicy,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default);

    Task<string> GetUrlAsync(string fileKey, CancellationToken cancellationToken = default);
}
