namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §17 (Faz 1b Görev 7): the public list/detail cache's TTL is normally 10 minutes, but is
// shortened when the item (or, for detail, one of its ancestors) has a PublishAtUtc/UnpublishAtUtc
// transition coming up sooner - so a scheduled publish/unpublish takes effect on the public site
// within seconds of its scheduled time, not up to 10 minutes late. Pure time math only; the caller
// gathers which DateTime? values are even relevant (its own and every ancestor's, for detail; just
// its own, for a list item - Görev 7's list endpoint uses the whole page's earliest transition).
public static class ContentCacheTtlCalculator
{
    public static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(10);

    private static readonly TimeSpan MinimumTtl = TimeSpan.FromSeconds(1);

    public static TimeSpan Calculate(DateTime now, IEnumerable<DateTime?> upcomingTransitions)
    {
        var earliestFutureTransition = upcomingTransitions
            .Where(t => t is not null && t.Value > now)
            .Select(t => t!.Value)
            .OrderBy(t => t)
            .Cast<DateTime?>()
            .FirstOrDefault();

        if (earliestFutureTransition is null)
        {
            return DefaultTtl;
        }

        var untilTransition = earliestFutureTransition.Value - now;
        return untilTransition < DefaultTtl ? (untilTransition < MinimumTtl ? MinimumTtl : untilTransition) : DefaultTtl;
    }
}
