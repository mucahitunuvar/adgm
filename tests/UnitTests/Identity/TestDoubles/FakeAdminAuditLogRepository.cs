using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;

namespace GenclikMerkezi.UnitTests.Identity.TestDoubles;

public sealed class FakeAdminAuditLogRepository : IAdminAuditLogRepository
{
    private readonly List<AdminAuditLogEntry> _entries = [];

    public IReadOnlyCollection<AdminAuditLogEntry> Entries => _entries.AsReadOnly();

    public void Add(AdminAuditLogEntry entry)
    {
        _entries.Add(entry);
    }
}
