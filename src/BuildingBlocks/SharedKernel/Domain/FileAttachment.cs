namespace GenclikMerkezi.SharedKernel.Domain;

// Generic metadata for a file stored behind IFileStorageService (ADR-019) - owned by whichever
// aggregate holds it (e.g. CandidateCv.Photo, CandidateCvContent.CvFile), never a standalone
// aggregate/table of its own. OwnerEntityType/OwnerEntityId are a plain string/Guid tag rather than
// a shared enum or FK, since the set of owning entity types spans modules this type must not depend
// on (AGENTS.md §35: Shared Kernel stays free of module-specific concepts).
public sealed class FileAttachment : ValueObject
{
    public string FileKey { get; }

    public string OriginalFileName { get; }

    public string ContentType { get; }

    public long SizeInBytes { get; }

    public DateTime UploadedAtUtc { get; }

    public string OwnerEntityType { get; }

    public Guid OwnerEntityId { get; }

    private FileAttachment(
        string fileKey,
        string originalFileName,
        string contentType,
        long sizeInBytes,
        DateTime uploadedAtUtc,
        string ownerEntityType,
        Guid ownerEntityId)
    {
        FileKey = fileKey;
        OriginalFileName = originalFileName;
        ContentType = contentType;
        SizeInBytes = sizeInBytes;
        UploadedAtUtc = uploadedAtUtc;
        OwnerEntityType = ownerEntityType;
        OwnerEntityId = ownerEntityId;
    }

    public static FileAttachment Create(
        string fileKey,
        string originalFileName,
        string contentType,
        long sizeInBytes,
        DateTime uploadedAtUtc,
        string ownerEntityType,
        Guid ownerEntityId) =>
        new(fileKey, originalFileName, contentType, sizeInBytes, uploadedAtUtc, ownerEntityType, ownerEntityId);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FileKey;
        yield return OriginalFileName;
        yield return ContentType;
        yield return SizeInBytes;
        yield return UploadedAtUtc;
        yield return OwnerEntityType;
        yield return OwnerEntityId;
    }
}
