using GenclikMerkezi.BuildingBlocks.Infrastructure.Events;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.ChangeUserRole;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;

namespace GenclikMerkezi.UnitTests.Identity.Features.ChangeUserRole;

public class UserRoleChangedAuditLogHandlerTests
{
    private readonly FakeAdminAuditLogRepository _auditLogRepository = new();
    private readonly FakeCurrentUserService _currentUserService = new() { UserId = Guid.NewGuid() };
    private readonly FakeUnitOfWork _unitOfWork = new();

    private UserRoleChangedAuditLogHandler CreateHandler() =>
        new(_auditLogRepository, _currentUserService, _unitOfWork);

    [Fact]
    public async Task Handle_WritesAuditEntryWithPreviousAndNewRoleInDetails()
    {
        var targetUserId = Guid.NewGuid();
        var domainEvent = new UserRoleChangedDomainEvent(targetUserId, UserRole.Candidate, UserRole.Employer);
        var notification = new DomainEventNotification<UserRoleChangedDomainEvent>(domainEvent);

        await CreateHandler().Handle(notification, CancellationToken.None);

        var entry = Assert.Single(_auditLogRepository.Entries);
        Assert.Equal(_currentUserService.UserId, entry.AdminUserId);
        Assert.Equal(targetUserId, entry.TargetUserId);
        Assert.Equal(AdminActionType.RoleChanged, entry.ActionType);
        Assert.Contains("Candidate", entry.Details);
        Assert.Contains("Employer", entry.Details);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }
}
