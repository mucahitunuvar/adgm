using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Modules.CareerAdvisor.Features.SendBulkNotification;
using GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;

namespace GenclikMerkezi.UnitTests.CareerAdvisor.Features.SendBulkNotification;

public class SendBulkNotificationCommandHandlerTests
{
    private readonly FakeIdentityService _identityService = new();
    private readonly FakeNotificationModuleContract _notificationModuleContract = new();

    private SendBulkNotificationCommandHandler CreateHandler() => new(_identityService, _notificationModuleContract);

    [Fact]
    public async Task Handle_WithMultipleRecipients_SendsBulkNotificationToAllResolvedProfiles()
    {
        var candidate1UserId = Guid.NewGuid();
        var candidate2UserId = Guid.NewGuid();
        _identityService.UserProfilesById[candidate1UserId] =
            new IdentityUserProfile(candidate1UserId, "aday1@example.com", "Ahmet", "Yılmaz", null);
        _identityService.UserProfilesById[candidate2UserId] =
            new IdentityUserProfile(candidate2UserId, "aday2@example.com", "Mehmet", "Demir", null);

        var result = await CreateHandler().Handle(
            new SendBulkNotificationCommand([candidate1UserId, candidate2UserId], "Duyuru", "Merhaba"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var sent = Assert.Single(_notificationModuleContract.SentBulkNotifications);
        Assert.Equal(2, sent.Recipients.Count);
        Assert.Contains(sent.Recipients, r => r.UserId == candidate1UserId && r.Email == "aday1@example.com");
        Assert.Contains(sent.Recipients, r => r.UserId == candidate2UserId && r.Email == "aday2@example.com");
        Assert.Equal("Duyuru", sent.Subject);
        Assert.Equal("Merhaba", sent.Message);
    }

    [Fact]
    public async Task Handle_WhenAProfileCannotBeResolved_SkipsItButStillSendsToTheRest()
    {
        var resolvableUserId = Guid.NewGuid();
        var unresolvableUserId = Guid.NewGuid();
        _identityService.UserProfilesById[resolvableUserId] =
            new IdentityUserProfile(resolvableUserId, "aday@example.com", "Ahmet", "Yılmaz", null);

        var result = await CreateHandler().Handle(
            new SendBulkNotificationCommand([resolvableUserId, unresolvableUserId], "Duyuru", "Merhaba"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var sent = Assert.Single(_notificationModuleContract.SentBulkNotifications);
        var recipient = Assert.Single(sent.Recipients);
        Assert.Equal(resolvableUserId, recipient.UserId);
    }
}
