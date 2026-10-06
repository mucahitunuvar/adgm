using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

// CleanupExpiredCookieConsentRecordsJobTests only needs GetRecordedBeforeAsync/Remove - the public
// create endpoint and the admin summary are covered by CookieConsentFlowTests (integration) instead,
// the same split FakeNewsletterSubscriberRepository documents for its own job-only surface.
public sealed class FakeCookieConsentRecordRepository : ICookieConsentRecordRepository
{
    private readonly List<CookieConsentRecord> _records = [];

    public void Seed(CookieConsentRecord record) => _records.Add(record);

    // Test-only helper (not part of ICookieConsentRecordRepository) - lets a test assert whether
    // ExecuteAsync removed a seeded record.
    public bool Contains(Guid id) => _records.Any(r => r.Id == id);

    public void Add(CookieConsentRecord record) => _records.Add(record);

    public void Remove(CookieConsentRecord record) => _records.Remove(record);

    public Task<IReadOnlyList<CookieConsentSummaryItem>> GetForSummaryAsync(
        DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not needed by job unit tests - covered by CookieConsentFlowTests (integration).");

    public Task<IReadOnlyList<CookieConsentRecord>> GetRecordedBeforeAsync(DateTime beforeUtc, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<CookieConsentRecord> due = _records.Where(r => r.RecordedAtUtc <= beforeUtc).ToList();
        return Task.FromResult(due);
    }
}
