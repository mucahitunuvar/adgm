namespace GenclikMerkezi.Modules.Employer.Features.GetPublicJobs;

// Görev 3 (Employer public jobs master prompt) §1: cinsiyet tercihi, firma iletişim/vergi/danışman
// bilgisi, inceleme/ret/revizyon notu yok. Askerlik durumu tercihi burada DA yok (yalnızca detayda).
public sealed record PublicJobListItemResponse(
    Guid Id,
    string Slug,
    string Title,
    PublicJobCompanyResponse Company,
    string? ProvinceName,
    string? EmploymentTypeName,
    string? WorkLocationTypeName,
    string? PositionName,
    bool IsForDisabledCandidates,
    DateTime PublishedAtUtc,
    string Summary);
