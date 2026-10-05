using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

// CleanupExpiredNewsletterSubscribersJobTests only needs GetPendingConfirmationOlderThanAsync/
// GetUnsubscribedOlderThanAsync/Remove - Search/export are covered by NewsletterSubscriptionFlowTests
// (integration) instead, the same split FakeFormSubmissionRepository documents for its own job-only
// surface.
public sealed class FakeNewsletterSubscriberRepository : INewsletterSubscriberRepository
{
    private readonly List<NewsletterSubscriber> _subscribers = [];

    public void Seed(NewsletterSubscriber subscriber) => _subscribers.Add(subscriber);

    public Task<NewsletterSubscriber?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_subscribers.FirstOrDefault(s => s.Id == id));

    public Task<NewsletterSubscriber?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        Task.FromResult(_subscribers.FirstOrDefault(s => s.Email == normalizedEmail));

    public Task<NewsletterSubscriber?> GetByUnsubscribeTokenAsync(string unsubscribeToken, CancellationToken cancellationToken = default) =>
        Task.FromResult(_subscribers.FirstOrDefault(s => s.UnsubscribeToken == unsubscribeToken));

    public void Add(NewsletterSubscriber subscriber) => _subscribers.Add(subscriber);

    public void Remove(NewsletterSubscriber subscriber) => _subscribers.Remove(subscriber);

    public Task<PagedResult<NewsletterSubscriberListItem>> SearchAsync(
        NewsletterSubscriberSearchFilter filter, PagedRequest pagedRequest, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not needed by job unit tests - covered by NewsletterSubscriptionFlowTests (integration).");

    public Task<IReadOnlyList<NewsletterSubscriberExportItem>> GetForExportAsync(
        string? status, string? language, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Not needed by job unit tests - covered by NewsletterSubscriptionFlowTests (integration).");

    public Task<IReadOnlyList<NewsletterSubscriber>> GetPendingConfirmationOlderThanAsync(
        DateTime subscribedBeforeUtc, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<NewsletterSubscriber> due = _subscribers
            .Where(s => s.Status == NewsletterSubscriberStatus.PendingConfirmation && s.SubscribedAtUtc <= subscribedBeforeUtc)
            .ToList();
        return Task.FromResult(due);
    }

    public Task<IReadOnlyList<NewsletterSubscriber>> GetUnsubscribedOlderThanAsync(
        DateTime unsubscribedBeforeUtc, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<NewsletterSubscriber> due = _subscribers
            .Where(s => s.Status == NewsletterSubscriberStatus.Unsubscribed && s.UnsubscribedAtUtc <= unsubscribedBeforeUtc)
            .ToList();
        return Task.FromResult(due);
    }
}
