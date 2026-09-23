using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.BuildingBlocks.Infrastructure.FileStorage;

// First IFileStorageService implementation (ADR-019): writes to one of two configurable root
// directories on local disk (ADR-019 Ek/ADR-024 Faz 0 Görev 4 - public vs. private, split by
// category), under {category}/{yyyy}/{MM}/{dd}/{guid}.{ext} (UTC date) within that root. File names
// are GUID-generated to avoid collisions; the caller-supplied original file name is preserved only
// in the returned FileAttachment, never used as the on-disk name. TimeProvider (rather than
// DateTime.UtcNow directly) makes the date partition deterministically testable. FileKey's format
// is unchanged by the root split - it is still just "{category}/{yyyy}/{MM}/{dd}/{guid}.{ext}",
// with no indication of which root it lives under; DeleteAsync/ReadAsync resolve that from the
// leading folder segment via FileCategoryExtensions.TryGetCategoryFromFolderSegment.
//
// A relative RootDirectory/PublicRootDirectory is resolved against IHostEnvironment.ContentRootPath
// (not Environment.CurrentDirectory, which File.* APIs would otherwise use implicitly) so that
// wherever the process's current working directory happens to be, this always agrees with
// Program.cs's own PhysicalFileProvider root for the public path - otherwise uploads and the static
// file middleware silently disagree on where a "public" file actually lives, and every public URL
// 404s despite the upload having "succeeded".
public sealed class LocalDiskFileStorageService(
    IOptions<FileStorageSettings> settings, TimeProvider timeProvider, IHostEnvironment environment)
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
        if (!category.TryGetFolderSegment(out var folderSegment) || !category.TryGetIsPublic(out var isPublic))
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
        var fullPath = ToFullPath(fileKey, isPublic);

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
        if (!TryResolveFullPath(fileKey, out var fullPath))
        {
            return Task.CompletedTask;
        }

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

    public async Task<byte[]?> ReadAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        if (!TryResolveFullPath(fileKey, out var fullPath) || !File.Exists(fullPath))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(fullPath, cancellationToken);
    }

    private bool TryResolveFullPath(string fileKey, out string fullPath)
    {
        var folderSegment = fileKey.Split('/', 2)[0];

        if (!FileCategoryExtensions.TryGetCategoryFromFolderSegment(folderSegment, out var category)
            || !category.TryGetIsPublic(out var isPublic))
        {
            fullPath = string.Empty;
            return false;
        }

        fullPath = ToFullPath(fileKey, isPublic);
        return true;
    }

    private string ToFullPath(string fileKey, bool isPublic)
    {
        var configuredRoot = isPublic ? settings.Value.PublicRootDirectory : settings.Value.RootDirectory;
        var root = Path.IsPathRooted(configuredRoot)
            ? configuredRoot
            : Path.Combine(environment.ContentRootPath, configuredRoot);
        return Path.Combine(root, fileKey.Replace('/', Path.DirectorySeparatorChar));
    }
}
