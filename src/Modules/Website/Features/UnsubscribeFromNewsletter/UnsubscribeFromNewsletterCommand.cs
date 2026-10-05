using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.UnsubscribeFromNewsletter;

public sealed record UnsubscribeFromNewsletterCommand(string? Token) : IRequest<Result>;
