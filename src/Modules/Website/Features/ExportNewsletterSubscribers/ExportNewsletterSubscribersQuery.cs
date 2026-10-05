using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ExportNewsletterSubscribers;

public sealed record ExportNewsletterSubscribersQuery(string? Status, string? Language)
    : IRequest<Result<IReadOnlyList<NewsletterSubscriberExportItem>>>;
