using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ExportNewsletterSubscribers;

public sealed class ExportNewsletterSubscribersQueryHandler(INewsletterSubscriberRepository newsletterSubscriberRepository)
    : IRequestHandler<ExportNewsletterSubscribersQuery, Result<IReadOnlyList<NewsletterSubscriberExportItem>>>
{
    public async Task<Result<IReadOnlyList<NewsletterSubscriberExportItem>>> Handle(
        ExportNewsletterSubscribersQuery request, CancellationToken cancellationToken)
    {
        var items = await newsletterSubscriberRepository.GetForExportAsync(request.Status, request.Language, cancellationToken);
        return Result.Success(items);
    }
}
