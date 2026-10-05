using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetNewsletterSubscribers;

public sealed record GetNewsletterSubscribersQuery(string? Status, string? Language, string? Search)
    : PagedRequest, IRequest<Result<PagedResult<NewsletterSubscriberSummaryResponse>>>;
