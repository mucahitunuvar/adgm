namespace GenclikMerkezi.SharedKernel.Domain;

// Single place mapping a FileCategory to its on-disk/URL folder segment (ADR-019) and to its
// public/private access type (ADR-024 Faz 0 Görev 4) - every IFileStorageService implementation
// (LocalDiskFileStorageService today, an S3-compatible one later) uses this instead of
// re-declaring either mapping. The Try* shape (rather than a plain switch expression that throws)
// lets callers turn an out-of-range enum value - e.g. one produced by casting an arbitrary int -
// into a Result.Failure instead of an unhandled exception.
public static class FileCategoryExtensions
{
    public static bool TryGetFolderSegment(this FileCategory category, out string folderSegment)
    {
        folderSegment = category switch
        {
            FileCategory.CandidatePhoto => "candidate-photos",
            FileCategory.CandidateCv => "candidate-cvs",
            FileCategory.EmployerLogo => "employer-logos",
            FileCategory.EmployerDocument => "employer-documents",
            FileCategory.WebsiteImage => "website-images",
            FileCategory.WebsiteDocument => "website-documents",
            FileCategory.WebsiteFormAttachment => "website-form-attachments",
            _ => string.Empty,
        };

        return folderSegment.Length > 0;
    }

    // The criterion is "is this ever shown on the public site", not "which module owns it" - e.g.
    // EmployerLogo is public (shown on the site) while WebsiteFormAttachment is private (personal
    // data submitted through a public form).
    public static bool TryGetIsPublic(this FileCategory category, out bool isPublic)
    {
        switch (category)
        {
            case FileCategory.EmployerLogo:
            case FileCategory.WebsiteImage:
            case FileCategory.WebsiteDocument:
                isPublic = true;
                return true;
            case FileCategory.CandidatePhoto:
            case FileCategory.CandidateCv:
            case FileCategory.EmployerDocument:
            case FileCategory.WebsiteFormAttachment:
                isPublic = false;
                return true;
            default:
                isPublic = false;
                return false;
        }
    }

    // Reverse of TryGetFolderSegment - resolves a FileKey's category from its leading folder
    // segment, so DeleteAsync/ReadAsync (which only receive a FileKey, not the original category)
    // can determine which physical root the file lives under.
    public static bool TryGetCategoryFromFolderSegment(string folderSegment, out FileCategory category)
    {
        foreach (var candidate in Enum.GetValues<FileCategory>())
        {
            if (candidate.TryGetFolderSegment(out var candidateSegment) && candidateSegment == folderSegment)
            {
                category = candidate;
                return true;
            }
        }

        category = default;
        return false;
    }
}
