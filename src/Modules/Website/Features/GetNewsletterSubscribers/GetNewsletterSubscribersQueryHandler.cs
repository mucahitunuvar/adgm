using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetNewsletterSubscribers;

public sealed class GetNewsletterSubscribersQueryHandler(INewsletterSubscriberRepository newsletterSubscriberRepository)
    : IRequestHandler<GetNewsletterSubscribersQuery, Result<PagedResult<NewsletterSubscriberSummaryResponse>>>
{
    public async Task<Result<PagedResult<NewsletterSubscriberSummaryResponse>>> Handle(
        GetNewsletterSubscribersQuery request, CancellationToken cancellationToken)
    {
        var filter = new NewsletterSubscriberSearchFilter(request.Status, request.Language, request.Search);
        var paged = await newsletterSubscriberRepository.SearchAsync(filter, request, cancellationToken);

        var items = paged.Items
            .Select(i => new NewsletterSubscriberSummaryResponse(i.Id, i.Email, i.LanguageCode, i.Status, i.SubscribedAtUtc, i.ConfirmedAtUtc))
            .ToList();

        return Result.Success(new PagedResult<NewsletterSubscriberSummaryResponse>(items, paged.TotalCount, paged.Page, paged.PageSize));
    }
}
