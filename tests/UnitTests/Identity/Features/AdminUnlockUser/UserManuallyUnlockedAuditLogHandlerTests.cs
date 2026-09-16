using GenclikMerkezi.BuildingBlocks.Infrastructure.Events;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.AdminUnlockUser;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;

namespace GenclikMerkezi.UnitTests.Identity.Features.AdminUnlockUser;

public class UserManuallyUnlockedAuditLogHandlerTests
{
    private readonly FakeAdminAuditLogRepository _auditLogRepository = new();
    private readonly FakeCurrentUserService _currentUserService = new() { UserId = Guid.NewGuid() };
    private readonly FakeUnitOfWork _unitOfWork = new();

    private UserManuallyUnlockedAuditLogHandler CreateHandler() =>
        new(_auditLogRepository, _currentUserService, _unitOfWork);

    [Fact]
    public async Task Handle_WritesAuditEntry()
    {
        var targetUserId = Guid.NewGuid();
        var notification = new DomainEventNotification<UserManuallyUnlockedDomainEvent>(
            new UserManuallyUnlockedDomainEvent(targetUserId));

        await CreateHandler().Handle(notification, CancellationToken.None);

        var entry = Assert.Single(_auditLogRepository.Entries);
        Assert.Equal(_currentUserService.UserId, entry.AdminUserId);
        Assert.Equal(targetUserId, entry.TargetUserId);
        Assert.Equal(AdminActionType.ManuallyUnlocked, entry.ActionType);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }
}
