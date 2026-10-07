namespace GenclikMerkezi.Modules.Employer.Application.Abstractions;

// Görev 3 (Employer public jobs master prompt): IJobRepository.SearchPublicJobsAsync'in Jobs+
// Companies join'inden tek sorguda projekte ettiği satır - lookup adları (il, çalışma şekli vb.)
// henüz çözülmemiş (handler, sayfadaki distinct id'leri IReferenceDataLookupReader ile toplu çözer).
public sealed record PublicJobListItem(
    Guid JobId,
    string? Slug,
    string Title,
    Guid CompanyId,
    string CompanyName,
    bool CompanyHasLogo,
    Guid ProvinceId,
    Guid EmploymentTypeId,
    Guid WorkLocationTypeId,
    Guid PositionId,
    bool IsForDisabledCandidates,
    DateTime PublishedAtUtc,
    string? DescriptionHtml);
