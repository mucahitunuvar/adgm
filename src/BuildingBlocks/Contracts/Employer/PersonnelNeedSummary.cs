namespace GenclikMerkezi.Contracts.Employer;

// CompanySummary ile aynı gerekçe (ADR-016 Decision 2): Contracts projesi Employer.Domain'e bağımlı
// olamaz, bu yüzden PersonnelNeedStatus taşınmıyor - yalnızca Genel Havuz sayfasının (CareerAdvisor
// Görev 6, henüz yazılmadı) ihtiyaç duyacağı düz alanlar.
public sealed record PersonnelNeedSummary(
    Guid Id,
    Guid CompanyId,
    Guid EmploymentTypeId,
    Guid WorkLocationTypeId,
    Guid PositionId,
    Guid DepartmentId,
    int Quantity,
    Guid ProvinceId,
    Guid ExperienceLevelId,
    string? DetailsText,
    Guid? PooledByAdvisorId,
    DateTime? PooledAtUtc);
