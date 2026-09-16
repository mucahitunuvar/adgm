using GenclikMerkezi.Modules.Identity.Domain;

namespace GenclikMerkezi.UnitTests.Identity.Domain;

public class AdminAuditLogEntryTests
{
    [Fact]
    public void Create_SetsAllFieldsAndGeneratesId()
    {
        var adminUserId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();

        var entry = AdminAuditLogEntry.Create(
            adminUserId, AdminActionType.Deactivated, targetUserId, "{}", "trace-1");

        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Equal(adminUserId, entry.AdminUserId);
        Assert.Equal(AdminActionType.Deactivated, entry.ActionType);
        Assert.Equal(targetUserId, entry.TargetUserId);
        Assert.Equal("{}", entry.Details);
        Assert.Equal("trace-1", entry.CorrelationId);
        Assert.True(entry.OccurredAtUtc <= DateTime.UtcNow);
    }

    [Fact]
    public void Create_WithoutDetailsOrCorrelationId_LeavesThemNull()
    {
        var entry = AdminAuditLogEntry.Create(Guid.NewGuid(), AdminActionType.ManuallyUnlocked, Guid.NewGuid());

        Assert.Null(entry.Details);
        Assert.Null(entry.CorrelationId);
    }
}
