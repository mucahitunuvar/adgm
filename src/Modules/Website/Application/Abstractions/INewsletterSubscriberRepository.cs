using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface INewsletterSubscriberRepository
{
    Task<NewsletterSubscriber?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<NewsletterSubscriber?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<NewsletterSubscriber?> GetByUnsubscribeTokenAsync(string unsubscribeToken, CancellationToken cancellationToken = default);

    void Add(NewsletterSubscriber subscriber);

    void Remove(NewsletterSubscriber subscriber);

    Task<PagedResult<NewsletterSubscriberListItem>> SearchAsync(
        NewsletterSubscriberSearchFilter filter, PagedRequest pagedRequest, CancellationToken cancellationToken = default);

    // §14 "GET .../export?status=Active&language=" - unpaged, scalar filters rather than the dedicated
    // filter record (only two fields, no Search), mirroring FormSubmissionRepository's own reasoning
    // for when a filter type earns its keep.
    Task<IReadOnlyList<NewsletterSubscriberExportItem>> GetForExportAsync(
        string? status, string? language, CancellationToken cancellationToken = default);

    // §14 retention job: "PendingConfirmation durumunda 7 gün içinde onaylanmayan kayıtlar silinir."
    Task<IReadOnlyList<NewsletterSubscriber>> GetPendingConfirmationOlderThanAsync(
        DateTime subscribedBeforeUtc, CancellationToken cancellationToken = default);

    // §14 retention job: "Unsubscribed kayıtlar 30 gün sonra silinir."
    Task<IReadOnlyList<NewsletterSubscriber>> GetUnsubscribedOlderThanAsync(
        DateTime unsubscribedBeforeUtc, CancellationToken cancellationToken = default);
}
