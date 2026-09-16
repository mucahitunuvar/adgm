using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Notification.Features.SendPasswordResetEmail;

public sealed record SendPasswordResetEmailCommand(
    Guid UserId,
    string Email,
    string ResetToken,
    DateTime ExpiresAtUtc) : IRequest<Result>;
