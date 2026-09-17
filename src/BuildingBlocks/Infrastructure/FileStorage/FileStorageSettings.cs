namespace GenclikMerkezi.BuildingBlocks.Infrastructure.FileStorage;

public sealed class FileStorageSettings
{
    public const string SectionName = "FileStorage";

    // Absolute or app-relative path LocalDiskFileStorageService writes uploaded files under.
    public string RootDirectory { get; init; } = "App_Data/uploads";

    // Prefix GetUrlAsync builds a file's public URL from: "{PublicBaseUrl}/{fileKey}".
    public string PublicBaseUrl { get; init; } = string.Empty;
}
