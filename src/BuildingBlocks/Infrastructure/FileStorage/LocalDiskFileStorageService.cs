using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.BuildingBlocks.Infrastructure.FileStorage;

// First IFileStorageService implementation (ADR-019): writes to a configurable root directory on
// local disk. File names are GUID-generated to avoid collisions; the caller-supplied original file
// name is preserved only in the returned FileAttachment, never used as the on-disk name.
public sealed class LocalDiskFileStorageService(IOptions<FileStorageSettings> settings) : IFileStorageService
{
    public async Task<Result<FileAttachment>> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        string folder,
        string ownerEntityType,
        Guid ownerEntityId,
        FileValidationPolicy validationPolicy,
        CancellationToken cancellationToken = default)
    {
        var validationResult = validationPolicy.Validate(fileName, contentType, content.Length);

        if (validationResult.IsFailure)
        {
            return Result.Failure<FileAttachment>(validationResult.Error);
        }

        var extension = Path.GetExtension(fileName);
        var fileKey = $"{folder}/{Guid.NewGuid():N}{extension}";
        var fullPath = ToFullPath(fileKey);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using (var fileStream = File.Create(fullPath))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        var attachment = FileAttachment.Create(
            fileKey, fileName, contentType, content.Length, DateTime.UtcNow, ownerEntityType, ownerEntityId);

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
