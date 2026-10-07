namespace GenclikMerkezi.Contracts.Employer;

// Görev 4 (Employer public jobs master prompt) §1: yalnızca public kurallara uyan (Published ilan +
// Approved firma) ilanlar bu özete girer; kişisel veri veya Employer iç alanı (danışman, inceleme/
// ret/revizyon notu, firma iletişim/vergi bilgisi, cinsiyet tercihi) yok. Lookup adları (il, çalışma
// şekli vb.) burada çözülmüş halde gelir - tüketen taraf (gelecekteki Website arama adaptörü)
// Employer'ın veya ReferenceData'nın hiçbir iç tipini bilmek zorunda kalmaz.
public sealed record PublishedJobSummary(
    Guid JobId,
    string Slug,
    string Title,
    string CompanyName,
    string? ProvinceName,
    string? EmploymentTypeName,
    string? WorkLocationTypeName,
    string? PositionName,
    string Summary,
    DateTime PublishedAtUtc);
