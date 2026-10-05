using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.SubscribeToNewsletter;

public sealed record SubscribeToNewsletterCommand(
    string? SubmissionToken,
    string? TurnstileToken,
    string? Website,
    string? Email,
    string? Lang,
    int? AcceptedPrivacyNoticeVersion,
    string? RemoteIpAddress) : IRequest<Result>;
