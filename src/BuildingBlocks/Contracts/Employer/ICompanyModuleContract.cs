namespace GenclikMerkezi.Contracts.Employer;

// The published, in-process contract other modules depend on instead of Employer's own DbContext
// (ADR-016 Decision 2, Option C - same pattern as ICareerAdvisorModuleContract/IIdentityService).
// Implemented in Employer.Infrastructure, registered once at the Host composition root. Kept
// deliberately minimal for this task (ADR-023 Görev 1) - Job/PersonnelNeed/Matching's future needs
// (e.g. resolving a Company's assigned advisor for Job review authorization) will extend this as
// those modules are built, not before.
public interface ICompanyModuleContract
{
    Task<CompanySummary?> GetCompanyByIdAsync(Guid companyId, CancellationToken cancellationToken = default);

    Task<Guid?> GetCareerAdvisorIdForCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);

    // CareerAdvisor Görev 6: Genel Havuz sayfasının bir sayfadaki distinct CompanyId kümesini toplu
    // çözmesi için - IReferenceDataLookupReader.GetByIdsAsync ile aynı semantik: bulunamayan id'ler
    // sonuçta sessizce yok, hata fırlatmaz.
    Task<IReadOnlyCollection<CompanySummary>> GetCompaniesByIdsAsync(
        IReadOnlyCollection<Guid> companyIds, CancellationToken cancellationToken = default);
}
