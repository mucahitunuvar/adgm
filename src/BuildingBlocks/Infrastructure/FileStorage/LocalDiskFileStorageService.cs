using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.BuildingBlocks.Infrastructure.FileStorage;

// First IFileStorageService implementation (ADR-019): writes to a configurable root directory on
// local disk, under UploadedFiles/{category}/{yyyy}/{MM}/{dd}/{guid}.{ext} (UTC date). File names
// are GUID-generated to avoid collisions; the caller-supplied original file name is preserved only
// in the returned FileAttachment, never used as the on-disk name. TimeProvider (rather than
// DateTime.UtcNow directly) makes the date partition deterministically testable.
public sealed class LocalDiskFileStorageService(IOptions<FileStorageSettings> settings, TimeProvider timeProvider)
    : IFileStorageService
{
    public async Task<Result<FileAttachment>> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        FileCategory category,
        string ownerEntityType,
        Guid ownerEntityId,
        FileValidationPolicy validationPolicy,
        CancellationToken cancellationToken = default)
    {
        if (!category.TryGetFolderSegment(out var folderSegment))
        {
            return Result.Failure<FileAttachment>(Error.Validation(
                "FileStorage.InvalidCategory", $"'{category}' is not a recognized file category."));
        }

        var validationResult = validationPolicy.Validate(fileName, contentType, content.Length);

        if (validationResult.IsFailure)
        {
            return Result.Failure<FileAttachment>(validationResult.Error);
        }

        var utcNow = timeProvider.GetUtcNow();
        var extension = Path.GetExtension(fileName);
        var fileKey = $"{folderSegment}/{utcNow:yyyy}/{utcNow:MM}/{utcNow:dd}/{Guid.NewGuid():N}{extension}";
        var fullPath = ToFullPath(fileKey);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using (var fileStream = File.Create(fullPath))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        var attachment = FileAttachment.Create(
            fileKey, fileName, contentType, content.Length, utcNow.UtcDateTime, ownerEntityType, ownerEntityId);

        return Result.Success(attachment);
    }

    public Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        var fullPath = ToFullPath(fileKey);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<string> GetUrlAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        var baseUrl = settings.Value.PublicBaseUrl.TrimEnd('/');
        return Task.FromResult($"{baseUrl}/{fileKey}");
    }

    private string ToFullPath(string fileKey) =>
        Path.Combine(settings.Value.RootDirectory, fileKey.Replace('/', Path.DirectorySeparatorChar));
}
