using GenclikMerkezi.Modules.Website.Application.Abstractions;
using Hangfire;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Jobs;

// ADR-024 §10 (Faz 5 Görev 2): ISearchIndexBatchScheduler's only implementation - the one place this
// module's search-index batching touches Hangfire directly, keeping SearchIndexUpdater (Application)
// free of any Infrastructure/third-party dependency (AGENTS.md §7/§28), same split
// HangfireEventCancellationBatchScheduler already uses.
public sealed class HangfireSearchIndexBatchScheduler(IBackgroundJobClient backgroundJobClient) : ISearchIndexBatchScheduler
{
    public void ScheduleSubtreeContinuation(Guid rootContentItemId, int skip) =>
        backgroundJobClient.Enqueue<ContinueSearchIndexBatchJob>(
            job => job.ContinueSubtreeAsync(rootContentItemId, skip, CancellationToken.None));

    public void ScheduleContentTypeContinuation(Guid contentTypeId, int page) =>
        backgroundJobClient.Enqueue<ContinueSearchIndexBatchJob>(
            job => job.ContinueContentTypeAsync(contentTypeId, page, CancellationToken.None));

    public void ScheduleLanguageContinuation(string languageCode, int page) =>
        backgroundJobClient.Enqueue<ContinueSearchIndexBatchJob>(
            job => job.ContinueLanguageAsync(languageCode, page, CancellationToken.None));
}
