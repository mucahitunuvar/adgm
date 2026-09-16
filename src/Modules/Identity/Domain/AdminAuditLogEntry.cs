using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Identity.Domain;

// Not part of the User aggregate (no navigation to it, never loaded/saved together): an audit
// record outlives and is queried independently of the account it describes, and the acting admin
// may differ from the target user. Append-only - there is no method to change an entry after
// creation.
public sealed class AdminAuditLogEntry : Entity
{
    public Guid AdminUserId { get; private set; }

    public AdminActionType ActionType { get; private set; }

    public Guid TargetUserId { get; private set; }

    public DateTime OccurredAtUtc { get; private set; }

    public string? Details { get; private set; }

    public string? CorrelationId { get; private set; }

    private AdminAuditLogEntry(
        Guid id,
        Guid adminUserId,
        AdminActionType actionType,
        Guid targetUserId,
        string? details,
        string? correlationId)
        : base(id)
    {
        AdminUserId = adminUserId;
        ActionType = actionType;
        TargetUserId = targetUserId;
        OccurredAtUtc = DateTime.UtcNow;
        Details = details;
        CorrelationId = correlationId;
    }

    private AdminAuditLogEntry()
    {
    }

    public static AdminAuditLogEntry Create(
        Guid adminUserId,
        AdminActionType actionType,
        Guid targetUserId,
        string? details = null,
        string? correlationId = null)
    {
        return new AdminAuditLogEntry(Guid.NewGuid(), adminUserId, actionType, targetUserId, details, correlationId);
    }
}
