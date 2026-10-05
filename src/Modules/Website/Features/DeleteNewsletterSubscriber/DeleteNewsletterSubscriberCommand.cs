using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.DeleteNewsletterSubscriber;

public sealed record DeleteNewsletterSubscriberCommand(Guid Id) : IRequest<Result>;
