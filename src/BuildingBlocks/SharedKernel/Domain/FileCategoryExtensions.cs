namespace GenclikMerkezi.SharedKernel.Domain;

// Single place mapping a FileCategory to its on-disk/URL folder segment (ADR-019) - every
// IFileStorageService implementation (LocalDiskFileStorageService today, an S3-compatible one
// later) uses this instead of re-declaring the mapping. TryGetFolderSegment (rather than a plain
// switch expression that throws) lets callers turn an out-of-range enum value - e.g. one produced
// by casting an arbitrary int - into a Result.Failure instead of an unhandled exception.
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
            _ => string.Empty,
        };

        return folderSegment.Length > 0;
    }
}
