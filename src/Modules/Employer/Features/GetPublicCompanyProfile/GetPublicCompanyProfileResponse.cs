namespace GenclikMerkezi.Modules.Employer.Features.GetPublicCompanyProfile;

// Görev 2 (Employer public jobs master prompt) §1: iletişim, adres, vergi, danışman, inceleme/ret/
// revizyon alanları yok - yalnızca kamuya açık bir firma profili sayfasının ihtiyaç duyduğu alanlar.
public sealed record GetPublicCompanyProfileResponse(
    Guid Id,
    string Name,
    string? SectorName,
    string? ProvinceName,
    int? FoundedYear,
    int? EmployeeCount,
    string? WebsiteUrl,
    string? AboutHtml,
    bool HasLogo,
    int PublishedJobCount);
