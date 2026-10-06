using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class CookieConsentRecordRepository(WebsiteDbContext dbContext) : ICookieConsentRecordRepository
{
    public void Add(CookieConsentRecord record) => dbContext.CookieConsentRecords.Add(record);

    public void Remove(CookieConsentRecord record) => dbContext.CookieConsentRecords.Remove(record);

    public async Task<IReadOnlyList<CookieConsentSummaryItem>> GetForSummaryAsync(
        DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default)
    {
        var query = dbContext.CookieConsentRecords.AsNoTracking().AsQueryable();

        if (fromUtc is not null)
        {
            query = query.Where(r => r.RecordedAtUtc >= fromUtc.Value);
        }

        if (toUtc is not null)
        {
            query = query.Where(r => r.RecordedAtUtc <= toUtc.Value);
        }

        var rows = await query.Select(r => new { r.Action, r.Categories }).ToListAsync(cancellationToken);
        return rows.Select(r => new CookieConsentSummaryItem(r.Action, r.Categories)).ToList();
    }

    public async Task<IReadOnlyList<CookieConsentRecord>> GetRecordedBeforeAsync(
        DateTime beforeUtc, CancellationToken cancellationToken = default) =>
        await dbContext.CookieConsentRecords.Where(r => r.RecordedAtUtc <= beforeUtc).ToListAsync(cancellationToken);
}
