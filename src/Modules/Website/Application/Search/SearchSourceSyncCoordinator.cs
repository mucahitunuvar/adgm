using System.Collections.Concurrent;

namespace GenclikMerkezi.Modules.Website.Application.Search;

// ADR-024 §10 (Faz 5 Görev 4): "Aynı kaynağın eşzamanlı iki senkronu engellenir" - a per-process,
// per-source-key in-memory lock shared by ExternalSearchSourceSynchronizer and
// WebsiteSearchIndexReconciler, so the scheduled Hangfire job and an admin's "anında yeniden
// indeksleme" command can never run the same source at once (whichever acquires the lock first wins;
// the other is told to back off - the job silently skips that source for this tick, the admin command
// surfaces it as 409). A single Hangfire server/single app instance is this project's deployment model
// (ARCHITECTURE.md), so a process-local lock is sufficient; a multi-instance deployment would need a
// database-level lock instead.
public sealed class SearchSourceSyncCoordinator
{
    private readonly ConcurrentDictionary<string, byte> _runningSourceKeys = new();

    public bool TryEnter(string sourceKey) => _runningSourceKeys.TryAdd(sourceKey, 0);

    public void Exit(string sourceKey) => _runningSourceKeys.TryRemove(sourceKey, out _);
}
