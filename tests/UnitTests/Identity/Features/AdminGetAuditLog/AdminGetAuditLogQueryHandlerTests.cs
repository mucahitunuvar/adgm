using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.AdminGetAuditLog;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;

namespace GenclikMerkezi.UnitTests.Identity.Features.AdminGetAuditLog;

public class AdminGetAuditLogQueryHandlerTests
{
    private readonly FakeAdminAuditLogRepository _auditLogRepository = new();

    private AdminGetAuditLogQueryHandler CreateHandler() => new(_auditLogRepository);

    private AdminAuditLogEntry AddEntry(Guid targetUserId, AdminActionType actionType)
    {
        var entry = AdminAuditLogEntry.Create(Guid.NewGuid(), actionType, targetUserId);
        _auditLogRepository.Add(entry);
        return entry;
    }

    [Fact]
    public async Task Handle_WithNoFilters_ReturnsAllEntriesPaged()
    {
        AddEntry(Guid.NewGuid(), AdminActionType.Deactivated);
        AddEntry(Guid.NewGuid(), AdminActionType.Reactivated);

        var result = await CreateHandler().Handle(
            new AdminGetAuditLogQuery(null, null, null, null, 1, 20), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
    }

    [Fact]
    public async Task Handle_FilteredByTargetUserId_ReturnsOnlyMatchingEntries()
    {
        var targetUserId = Guid.NewGuid();
        AddEntry(targetUserId, AdminActionType.Deactivated);
        AddEntry(Guid.NewGuid(), AdminActionType.Deactivated);

        var result = await CreateHandler().Handle(
            new AdminGetAuditLogQuery(targetUserId, null, null, null, 1, 20), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal(targetUserId, result.Value.Items[0].TargetUserId);
    }

    [Fact]
    public async Task Handle_FilteredByActionType_ReturnsOnlyMatchingEntries()
    {
        AddEntry(Guid.NewGuid(), AdminActionType.RoleChanged);
        AddEntry(Guid.NewGuid(), AdminActionType.ManuallyUnlocked);

        var result = await CreateHandler().Handle(
            new AdminGetAuditLogQuery(null, "RoleChanged", null, null, 1, 20), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal("RoleChanged", result.Value.Items[0].ActionType);
    }

    [Fact]
    public async Task Handle_FilteredByDateRange_ReturnsOnlyEntriesWithinRange()
    {
        var oldEntry = AddEntry(Guid.NewGuid(), AdminActionType.Deactivated);
        await Task.Delay(20);
        var boundary = DateTime.UtcNow;
        await Task.Delay(20);
        AddEntry(Guid.NewGuid(), AdminActionType.Reactivated);

        var result = await CreateHandler().Handle(
            new AdminGetAuditLogQuery(null, null, oldEntry.OccurredAtUtc.AddMilliseconds(-1), boundary, 1, 20),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items);
        Assert.Equal(oldEntry.Id, result.Value.Items[0].Id);
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectSliceAndTotals()
    {
        for (var i = 0; i < 5; i++)
        {
            AddEntry(Guid.NewGuid(), AdminActionType.Deactivated);
        }

        var result = await CreateHandler().Handle(
            new AdminGetAuditLogQuery(null, null, null, null, 2, 2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value.TotalCount);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(3, result.Value.TotalPages);
    }
}
