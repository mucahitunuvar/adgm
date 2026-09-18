namespace GenclikMerkezi.Modules.Candidate.Application.Abstractions;

// Fully display-ready input for ICandidateCvPdfExportService (ADR-021): every lookup id has already
// been resolved to its display name (via IReferenceDataLookupReader) by the feature handler that
// builds this - the PDF-rendering side (Infrastructure) never touches ReferenceData or any
// repository itself, it only lays this data out.
public sealed record CandidateCvPdfModel(
    byte[]? PhotoBytes,
    string FullName,
    string? Title,
    string Email,
    string? PhoneNumber,
    string? Address,
    string? ProvinceName,
    string? DistrictName,
    IReadOnlyList<CandidateCvPdfSocialMediaLink> SocialMediaLinks,
    DateOnly? BirthDate,
    string? GenderName,
    string? NationalityName,
    string? DriversLicenseTypeName,
    string? MilitaryStatusName,
    CandidateCvPdfDisabilityInfo? DisabilityInfo,
    string? Summary,
    IReadOnlyList<CandidateCvPdfExperience> Experiences,
    IReadOnlyList<CandidateCvPdfEducation> Educations,
    string? ComputerSkills,
    IReadOnlyList<CandidateCvPdfLanguage> Languages,
    IReadOnlyList<CandidateCvPdfCertificate> Certificates,
    IReadOnlyList<CandidateCvPdfReference> References,
    string? Hobbies);
