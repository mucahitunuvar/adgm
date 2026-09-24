using GenclikMerkezi.Modules.Notification.Domain;
using GenclikMerkezi.Modules.Notification.Infrastructure;
using GenclikMerkezi.UnitTests.Notification.TestDoubles;

namespace GenclikMerkezi.UnitTests.Notification.Infrastructure;

public class NotificationModuleContractTests
{
    private readonly FakeEmailSender _emailSender = new();
    private readonly FakeEmailNotificationRepository _emailNotificationRepository = new();
    private readonly FakeInAppNotificationRepository _inAppNotificationRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private NotificationModuleContract CreateContract() =>
        new(_emailSender, _emailNotificationRepository, _inAppNotificationRepository, _unitOfWork);

    [Fact]
    public async Task SendAsync_WithSuccessfulEmailDelivery_RecordsSentEmailAndInAppNotification()
    {
        var userId = Guid.NewGuid();

        await CreateContract().SendAsync(userId, "aday@example.com", "Konu", "Mesaj", CancellationToken.None);

        var emailNotification = Assert.Single(_emailNotificationRepository.Notifications);
        Assert.Equal("aday@example.com", emailNotification.ToEmail);
        Assert.Equal("Konu", emailNotification.Subject);
        Assert.Equal(EmailNotificationStatus.Sent, emailNotification.Status);

        var inAppNotification = Assert.Single(_inAppNotificationRepository.Notifications);
        Assert.Equal(userId, inAppNotification.UserId);
        Assert.Equal("Mesaj", inAppNotification.Message);

        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task SendAsync_WhenEmailDeliveryFails_StillRecordsInAppNotification_AndMarksEmailFailed()
    {
        _emailSender.ThrowOnSend = new InvalidOperationException("SMTP unreachable");

        await CreateContract().SendAsync(Guid.NewGuid(), "aday@example.com", "Konu", "Mesaj", CancellationToken.None);

        var emailNotification = Assert.Single(_emailNotificationRepository.Notifications);
        Assert.Equal(EmailNotificationStatus.Failed, emailNotification.Status);
        Assert.Equal("SMTP unreachable", emailNotification.FailureReason);

        Assert.Single(_inAppNotificationRepository.Notifications);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task SendEmailAsync_WithSuccessfulDelivery_RecordsSentEmail_WithoutAnyInAppNotification()
    {
        await CreateContract().SendEmailAsync("ziyaretci@example.com", "Konu", "Mesaj", CancellationToken.None);

        var emailNotification = Assert.Single(_emailNotificationRepository.Notifications);
        Assert.Equal("ziyaretci@example.com", emailNotification.ToEmail);
        Assert.Equal(EmailNotificationStatus.Sent, emailNotification.Status);

        Assert.Empty(_inAppNotificationRepository.Notifications);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task SendEmailAsync_WhenDeliveryFails_MarksEmailFailed_ButDoesNotThrow()
    {
        _emailSender.ThrowOnSend = new InvalidOperationException("SMTP unreachable");

        await CreateContract().SendEmailAsync("ziyaretci@example.com", "Konu", "Mesaj", CancellationToken.None);

        var emailNotification = Assert.Single(_emailNotificationRepository.Notifications);
        Assert.Equal(EmailNotificationStatus.Failed, emailNotification.Status);
        Assert.Equal("SMTP unreachable", emailNotification.FailureReason);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }
}
