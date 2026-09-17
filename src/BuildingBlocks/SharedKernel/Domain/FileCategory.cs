namespace GenclikMerkezi.SharedKernel.Domain;

// Closed set of categories IFileStorageService.UploadAsync accepts (ADR-019's folder structure:
// UploadedFiles/{category}/{yyyy}/{MM}/{dd}/{guid}.{ext}). A fixed enum rather than a free-form
// string so a typo can never create a stray folder on disk. Employer categories are declared ahead
// of the Employer module existing, so that module only adds usages later, not enum members.
public enum FileCategory
{
    CandidatePhoto,
    CandidateCv,
    EmployerLogo,
    EmployerDocument,
}
