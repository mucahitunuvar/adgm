using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Contracts.Employer;

// The published, in-process contract other modules depend on instead of Employer's own DbContext
// (ADR-016 Decision 2, Option C - same pattern as ICompanyModuleContract/ICareerAdvisorModuleContract).
// Implemented in Employer.Infrastructure, registered once at the Host composition root. Görev 4
// (Employer public jobs master prompt): Website does not know about this contract yet - the
// adapter that calls it (ADR-024 §10's IExternalSearchSource implementation, pulled by a Website
// Hangfire recurring job) is written in Host as part of WEBSITE-FAZ5-MASTER-PROMPT, after this.
//
// Sıralama kararlı (ADR-024 §10: "o kaynağa ait SearchDocument kümesini tam olarak senkronlar" -
// bir full-sync job'ı sayfalar arasında kayıp/çift olmadan tüm kaynağı taramalı):
// PublishedAtUtc artan, sonra Id artan - GetPublicJobsQuery'nin (Görev 3) listeleme amaçlı "en
// yeni önce" sıralamasından kasıtlı olarak farklı.
public interface IPublishedJobModuleContract
{
    Task<PagedResult<PublishedJobSummary>> GetPublishedJobSummariesAsync(
        int page, int pageSize, CancellationToken cancellationToken = default);
}
