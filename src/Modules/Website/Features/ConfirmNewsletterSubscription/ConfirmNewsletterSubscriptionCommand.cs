using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ConfirmNewsletterSubscription;

public sealed record ConfirmNewsletterSubscriptionCommand(string? Token) : IRequest<Result>;
