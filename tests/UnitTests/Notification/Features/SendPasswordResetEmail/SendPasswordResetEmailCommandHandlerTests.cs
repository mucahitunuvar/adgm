using GenclikMerkezi.Modules.Notification.Application;
using GenclikMerkezi.Modules.Notification.Domain;
using GenclikMerkezi.Modules.Notification.Features.SendPasswordResetEmail;
using GenclikMerkezi.UnitTests.Notification.TestDoubles;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.Notification.Features.SendPasswordResetEmail;

public class SendPasswordResetEmailCommandHandlerTests
{
    private readonly FakeEmailSender _emailSender = new();
    private readonly FakeEmailNotificationRepository _emailNotificationRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private SendPasswordResetEmailCommandHandler CreateHandler() =>
        new(
            _emailSender,
            _emailNotificationRepository,
            Options.Create(new AppLinkSettings { ApiBaseUrl = "https://test.example.com" }),
            _unitOfWork);

    [Fact]
    public async Task Handle_WhenEmailSendsSuccessfully_RecordsNotificationAsSent()
    {
        var command = new SendPasswordResetEmailCommand(
            Guid.NewGuid(), "aday@example.com", "plain-reset-token", DateTime.UtcNow.AddMinutes(30));

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var sentEmail = Assert.Single(_emailSender.SentEmails);
        Assert.Equal("aday@example.com", sentEmail.ToEmail);
        Assert.Contains("plain-reset-token", sentEmail.Body);

        var notification = Assert.Single(_emailNotificationRepository.Notifications);
        Assert.Equal(EmailNotificationStatus.Sent, notification.Status);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenEmailSendingThrows_RecordsNotificationAsFailed_ButStillSucceeds()
    {
        _emailSender.ThrowOnSend = new InvalidOperationException("SMTP unreachable");
        var command = new SendPasswordResetEmailCommand(
            Guid.NewGuid(), "aday@example.com", "plain-reset-token", DateTime.UtcNow.AddMinutes(30));

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(_emailSender.SentEmails);

        var notification = Assert.Single(_emailNotificationRepository.Notifications);
        Assert.Equal(EmailNotificationStatus.Failed, notification.Status);
        Assert.Equal("SMTP unreachable", notification.FailureReason);
    }
}
