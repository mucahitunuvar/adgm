namespace GenclikMerkezi.Modules.CareerAdvisor.Features.GetGeneralPool;

// Firma/lookup adları toplu çözülür (SearchCandidatesQueryHandler deseni); bir id çözülemezse
// (silinmiş/bulunamayan kayıt) yalnızca ilgili alan null kalır, satır response'tan düşmez.
public sealed record PersonnelNeedPoolItemResponse(
    Guid Id,
    string? CompanyName,
    string? PositionName,
    string? DepartmentName,
    string? ProvinceName,
    string? EmploymentTypeName,
    string? WorkLocationTypeName,
    string? ExperienceLevelName,
    int Quantity,
    string? DetailsText,
    DateTime? PooledAtUtc);
