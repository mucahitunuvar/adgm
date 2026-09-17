using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.SharedKernel.Domain;

// Centralized validation policy for IFileStorageService.UploadAsync (ADR-019): the enforcement
// algorithm (Validate) lives once here, in SharedKernel, so every current and future
// IFileStorageService implementation (local disk today, S3 later) shares it rather than
// reimplementing it - but each call site supplies its own limits (e.g. Candidate's photo policy vs.
// its CV file policy), so no module is stuck with another module's limits.
public sealed class FileValidationPolicy : ValueObject
{
    public IReadOnlyCollection<string> AllowedExtensions { get; }

    public IReadOnlyCollection<string> AllowedContentTypes { get; }

    public long MaxSizeInBytes { get; }

    private FileValidationPolicy(
        IReadOnlyCollection<string> allowedExtensions,
        IReadOnlyCollection<string> allowedContentTypes,
        long maxSizeInBytes)
    {
        AllowedExtensions = allowedExtensions;
        AllowedContentTypes = allowedContentTypes;
        MaxSizeInBytes = maxSizeInBytes;
    }

    // Extensions/content types are matched case-insensitively; pass extensions without their
    // leading dot ("jpg", not ".jpg").
    public static FileValidationPolicy Create(
        IEnumerable<string> allowedExtensions, IEnumerable<string> allowedContentTypes, long maxSizeInBytes)
    {
        return new FileValidationPolicy(
            allowedExtensions.Select(e => e.TrimStart('.').ToLowerInvariant()).ToArray(),
            allowedContentTypes.Select(c => c.ToLowerInvariant()).ToArray(),
            maxSizeInBytes);
    }

    public Result Validate(string fileName, string contentType, long sizeInBytes)
    {
        var extension = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();

        if (extension.Length == 0 || !AllowedExtensions.Contains(extension))
        {
            return Result.Failure(Error.Validation(
                "FileStorage.InvalidExtension",
                $"File extension '.{extension}' is not allowed. Allowed extensions: {string.Join(", ", AllowedExtensions)}."));
        }

        if (!AllowedContentTypes.Contains(contentType.ToLowerInvariant()))
        {
            return Result.Failure(Error.Validation(
                "FileStorage.InvalidContentType",
                $"Content type '{contentType}' is not allowed. Allowed content types: {string.Join(", ", AllowedContentTypes)}."));
        }

        if (sizeInBytes > MaxSizeInBytes)
        {
            return Result.Failure(Error.Validation(
                "FileStorage.FileTooLarge",
                $"File size {sizeInBytes} bytes exceeds the maximum allowed size of {MaxSizeInBytes} bytes."));
        }

        return Result.Success();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return string.Join(',', AllowedExtensions);
        yield return string.Join(',', AllowedContentTypes);
        yield return MaxSizeInBytes;
    }
}
