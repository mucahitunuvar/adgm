namespace GenclikMerkezi.BuildingBlocks.Infrastructure.FileStorage;

public sealed class FileStorageSettings
{
    public const string SectionName = "FileStorage";

    // Absolute or app-relative path LocalDiskFileStorageService writes private-category files
    // under (ADR-019 Ek/ADR-024 Faz 0 Görev 4). Never served statically by Host.
    public string RootDirectory { get; init; } = "App_Data/uploads";

    // Absolute or app-relative path LocalDiskFileStorageService writes public-category files
    // under. Host serves this root statically at PublicRequestPath.
    public string PublicRootDirectory { get; init; } = "App_Data/webuploads";

    // The request path Host's static file middleware maps to PublicRootDirectory.
    public string PublicRequestPath { get; init; } = "/webuploads";

    // Prefix GetUrlAsync builds a file's URL from: "{PublicBaseUrl}/{fileKey}", for every category
    // alike (GetUrlAsync is not category-aware - ADR-019 Ek). Only public-category URLs actually
    // resolve; private-category URLs remain unservable, same as before this split.
    public string PublicBaseUrl { get; init; } = string.Empty;
}
