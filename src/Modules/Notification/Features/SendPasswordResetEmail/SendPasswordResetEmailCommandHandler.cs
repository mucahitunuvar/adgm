using GenclikMerkezi.Modules.Notification.Application;
using GenclikMerkezi.Modules.Notification.Application.Abstractions;
using GenclikMerkezi.Modules.Notification.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.Modules.Notification.Features.SendPasswordResetEmail;

public sealed class SendPasswordResetEmailCommandHandler(
    IEmailSender emailSender,
    IEmailNotificationRepository emailNotificationRepository,
    IOptions<AppLinkSettings> appLinkSettings,
    [FromKeyedServices(NotificationModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<SendPasswordResetEmailCommand, Result>
{
    public async Task<Result> Handle(SendPasswordResetEmailCommand request, CancellationToken cancellationToken)
    {
        // No frontend page exists yet to carry this token and collect a new password (ADR-010:
        // Backend-First), so the email states the token itself alongside the API call it maps to,
        // rather than a clickable link like email verification's GET-based flow.
        const string subject = "Şifre sıfırlama talebiniz";
        var body = $"""
            <p>Merhaba,</p>
            <p>Gençlik Merkezi hesabınız için bir şifre sıfırlama talebinde bulunuldu.</p>
            <p>Bu talebi siz yapmadıysanız bu e-postayı yok sayabilirsiniz.</p>
            <p>Sıfırlama kodunuz: <strong>{request.ResetToken}</strong></p>
            <p>Bu kod {request.ExpiresAtUtc:R} (UTC) tarihine kadar geçerlidir.</p>
            <p>API üzerinden: <code>POST {appLinkSettings.Value.ApiBaseUrl}/api/v1/auth/reset-password</code></p>
            """;

        var notification = EmailNotification.Create(request.Email, subject, body);

        // Failures are recorded (not rethrown) so the message is not retried indefinitely by CAP
        // for what may be a permanent failure (e.g. an invalid address); the EmailNotification
        // audit trail is how a failed delivery gets noticed and followed up on for now.
        try
        {
            await emailSender.SendAsync(request.Email, subject, body, cancellationToken);
            notification.MarkSent();
        }
        catch (Exception ex)
        {
            notification.MarkFailed(ex.Message);
        }

        emailNotificationRepository.Add(notification);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
