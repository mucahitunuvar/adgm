using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.CreateEventRegistration;

public sealed record CreateEventRegistrationCommand(
    Guid ContentItemId,
    string? SubmissionToken,
    string? TurnstileToken,
    string? Website,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? Lang,
    int? AcceptedPrivacyNoticeVersion,
    string? RemoteIpAddress) : IRequest<Result>;
