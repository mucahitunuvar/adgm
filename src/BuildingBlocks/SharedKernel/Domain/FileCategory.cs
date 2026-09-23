namespace GenclikMerkezi.SharedKernel.Domain;

// Closed set of categories IFileStorageService.UploadAsync accepts (ADR-019's folder structure:
// UploadedFiles/{category}/{yyyy}/{MM}/{dd}/{guid}.{ext}). A fixed enum rather than a free-form
// string so a typo can never create a stray folder on disk. Employer/Website categories are
// declared ahead of their module's own upload feature existing, so that module only adds usages
// later, not enum members.
//
// ADR-024 Faz 0 Görev 4: each category also has a fixed public/private access type (see
// FileCategoryExtensions.TryGetIsPublic) determining which of the two physical storage roots it is
// written under and whether Host serves it statically at all.
public enum FileCategory
{
    CandidatePhoto,
    CandidateCv,
    EmployerLogo,
    EmployerDocument,
    WebsiteImage,
    WebsiteDocument,
    WebsiteFormAttachment,
}
