using GenclikMerkezi.Modules.Notification.Application;
using GenclikMerkezi.Modules.Notification.Application.Abstractions;
using GenclikMerkezi.Modules.Notification.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.Modules.Notification.Features.SendVerificationEmail;

public sealed class SendVerificationEmailCommandHandler(
    IEmailSender emailSender,
    IEmailNotificationRepository emailNotificationRepository,
    IOptions<AppLinkSettings> appLinkSettings,
    [FromKeyedServices(NotificationModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<SendVerificationEmailCommand, Result>
{
    public async Task<Result> Handle(SendVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        var verificationLink =
            $"{appLinkSettings.Value.ApiBaseUrl}/api/v1/auth/verify-email?token={Uri.EscapeDataString(request.VerificationToken)}";

        const string subject = "E-posta adresinizi doğrulayın";
        var body = $"""
            <p>Merhaba,</p>
            <p>Gençlik Merkezi hesabınızı doğrulamak için aşağıdaki bağlantıya tıklayın:</p>
            <p><a href="{verificationLink}">{verificationLink}</a></p>
            <p>Bu bağlantı {request.ExpiresAtUtc:R} (UTC) tarihine kadar geçerlidir.</p>
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
