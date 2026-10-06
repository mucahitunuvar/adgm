using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.ResendEventRegistrationVerification;

public sealed record ResendEventRegistrationVerificationCommand(
    Guid ContentItemId,
    string? SubmissionToken,
    string? TurnstileToken,
    string? Website,
    string? Email,
    string? RemoteIpAddress) : IRequest<Result>;
