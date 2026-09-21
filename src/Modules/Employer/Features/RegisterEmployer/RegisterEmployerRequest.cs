namespace GenclikMerkezi.Modules.Employer.Features.RegisterEmployer;

// Employer.md'deki Firma Bilgileri + Hesap Bilgileri alanlarının tamamı (Logo hariç - ayrı bir upload
// adımı, UploadCandidatePhoto deseniyle sonradan eklenecek). Candidate'ın aksine (minimal kayıt,
// profil sonradan dolduruluyor) Employer.md tek bir "firma profili" formu tanımladığı için bu bilinçli
// bir sapma (master prompt madde 3).
public sealed record RegisterEmployerRequest(
    string Email,
    string Password,
    string Name,
    Guid SectorId,
    int? FoundedYear,
    int? EmployeeCount,
    string? WebsiteUrl,
    Guid CountryId,
    Guid ProvinceId,
    Guid DistrictId,
    string Address,
    string? AboutHtml,
    string ContactFirstName,
    string ContactLastName,
    string ContactPhone,
    Guid TaxOfficeId,
    string TaxNumber,
    bool MarketingConsent);
