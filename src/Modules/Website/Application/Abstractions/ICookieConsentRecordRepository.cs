using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface ICookieConsentRecordRepository
{
    void Add(CookieConsentRecord record);

    void Remove(CookieConsentRecord record);

    Task<IReadOnlyList<CookieConsentSummaryItem>> GetForSummaryAsync(
        DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default);

    // CleanupExpiredCookieConsentRecordsJob (ADR-024 §13/SECURITY.md: 3-year retention).
    Task<IReadOnlyList<CookieConsentRecord>> GetRecordedBeforeAsync(DateTime beforeUtc, CancellationToken cancellationToken = default);
}
