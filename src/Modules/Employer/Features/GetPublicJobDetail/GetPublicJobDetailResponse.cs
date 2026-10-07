using GenclikMerkezi.Modules.Employer.Features.GetPublicJobs;

namespace GenclikMerkezi.Modules.Employer.Features.GetPublicJobDetail;

// Görev 3 (Employer public jobs master prompt) §1: cinsiyet tercihi, firma iletişim/vergi/danışman
// bilgisi, inceleme/ret/revizyon notu hiçbir public yanıtta yok. Askerlik durumu tercihi adları
// (MilitaryStatusPreferenceNames) yalnızca burada var - listede/özette yok (kullanıcı kararı).
public sealed record GetPublicJobDetailResponse(
    Guid Id,
    string Slug,
    string Title,
    PublicJobCompanyResponse Company,
    string? ProvinceName,
    string? EmploymentTypeName,
    string? WorkLocationTypeName,
    string? PositionName,
    string? DepartmentName,
    bool IsForDisabledCandidates,
    DateTime PublishedAtUtc,
    string? DescriptionHtml,
    string? ExperienceLevelName,
    IReadOnlyList<string> EducationLevelNames,
    IReadOnlyList<string> DrivingLicenseNames,
    IReadOnlyList<string> MilitaryStatusPreferenceNames,
    IReadOnlyList<PublicJobLanguageRequirementResponse> LanguageRequirements);
