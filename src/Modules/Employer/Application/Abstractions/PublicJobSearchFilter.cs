namespace GenclikMerkezi.Modules.Employer.Application.Abstractions;

// Görev 3 (Employer public jobs master prompt): GetPublicJobsQueryHandler'dan IJobRepository.
// SearchPublicJobsAsync'e geçirilen filtre - sekiz ayrı parametre yerine tek bir tip (AGENTS.md §16:
// repository anlamlı persistence operasyonları ifade etmeli).
public sealed record PublicJobSearchFilter(
    Guid? ProvinceId,
    Guid? EmploymentTypeId,
    Guid? WorkLocationTypeId,
    Guid? PositionId,
    Guid? DepartmentId,
    Guid? CompanyId,
    bool? IsForDisabledCandidates,
    string? Search);
