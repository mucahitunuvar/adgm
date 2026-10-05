using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Persistence;

public sealed class NewsletterSubscriberRepository(WebsiteDbContext dbContext) : INewsletterSubscriberRepository
{
    public Task<NewsletterSubscriber?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.NewsletterSubscribers.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<NewsletterSubscriber?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        dbContext.NewsletterSubscribers.FirstOrDefaultAsync(s => s.Email == normalizedEmail, cancellationToken);

    public Task<NewsletterSubscriber?> GetByUnsubscribeTokenAsync(string unsubscribeToken, CancellationToken cancellationToken = default) =>
        dbContext.NewsletterSubscribers.FirstOrDefaultAsync(s => s.UnsubscribeToken == unsubscribeToken, cancellationToken);

    public void Add(NewsletterSubscriber subscriber) => dbContext.NewsletterSubscribers.Add(subscriber);

    public void Remove(NewsletterSubscriber subscriber) => dbContext.NewsletterSubscribers.Remove(subscriber);

    public async Task<PagedResult<NewsletterSubscriberListItem>> SearchAsync(
        NewsletterSubscriberSearchFilter filter, PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        var query = dbContext.NewsletterSubscribers.AsNoTracking().AsQueryable();

        var filterResult = ApplyFilters(query, filter.Status, filter.Language);
        if (filterResult.IsFailure)
        {
            return new PagedResult<NewsletterSubscriberListItem>([], 0, pagedRequest.Page, pagedRequest.PageSize);
        }

        query = filterResult.Value;

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(s => s.Email.Contains(filter.Search));
        }

        var projected = query
            .OrderByDescending(s => s.SubscribedAtUtc)
            .Select(s => new NewsletterSubscriberListItem(s.Id, s.Email, s.LanguageCode.Value, s.Status.ToString(), s.SubscribedAtUtc, s.ConfirmedAtUtc));

        return await projected.ToPagedResultAsync(pagedRequest, cancellationToken);
    }

    public async Task<IReadOnlyList<NewsletterSubscriberExportItem>> GetForExportAsync(
        string? status, string? language, CancellationToken cancellationToken = default)
    {
        var query = dbContext.NewsletterSubscribers.AsNoTracking().AsQueryable();

        var filterResult = ApplyFilters(query, status, language);
        if (filterResult.IsFailure)
        {
            return [];
        }

        return await filterResult.Value
            .OrderBy(s => s.Email)
            .Select(s => new NewsletterSubscriberExportItem(s.Email, s.LanguageCode.Value, s.ConfirmedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NewsletterSubscriber>> GetPendingConfirmationOlderThanAsync(
        DateTime subscribedBeforeUtc, CancellationToken cancellationToken = default) =>
        await dbContext.NewsletterSubscribers
            .Where(s => s.Status == NewsletterSubscriberStatus.PendingConfirmation && s.SubscribedAtUtc <= subscribedBeforeUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<NewsletterSubscriber>> GetUnsubscribedOlderThanAsync(
        DateTime unsubscribedBeforeUtc, CancellationToken cancellationToken = default) =>
        await dbContext.NewsletterSubscribers
            .Where(s => s.Status == NewsletterSubscriberStatus.Unsubscribed && s.UnsubscribedAtUtc <= unsubscribedBeforeUtc)
            .ToListAsync(cancellationToken);

    // An unparsable status/language filter can never match a real row - short-circuit callers to an
    // empty result rather than translating a failed parse into a query, the same reasoning
    // FormSubmissionRepository.SearchAsync applies to its own FormKey/Status filters.
    private static Result<IQueryable<NewsletterSubscriber>> ApplyFilters(
        IQueryable<NewsletterSubscriber> query, string? status, string? language)
    {
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<NewsletterSubscriberStatus>(status, out var parsedStatus))
            {
                return Result.Failure<IQueryable<NewsletterSubscriber>>(Error.Validation("NewsletterSubscriber.InvalidStatusFilter", "Invalid status."));
            }

            query = query.Where(s => s.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(language))
        {
            var languageResult = LanguageCode.Create(language);
            if (languageResult.IsFailure)
            {
                return Result.Failure<IQueryable<NewsletterSubscriber>>(languageResult.Error);
            }

            var languageCode = languageResult.Value;
            query = query.Where(s => s.LanguageCode == languageCode);
        }

        return Result.Success(query);
    }
}
