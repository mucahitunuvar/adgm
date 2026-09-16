using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Notification.Features.SendVerificationEmail;

public sealed record SendVerificationEmailCommand(
    Guid UserId,
    string Email,
    string VerificationToken,
    DateTime ExpiresAtUtc) : IRequest<Result>;
