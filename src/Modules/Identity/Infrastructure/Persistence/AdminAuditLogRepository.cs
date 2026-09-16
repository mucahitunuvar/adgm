using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;

namespace GenclikMerkezi.Modules.Identity.Infrastructure.Persistence;

public sealed class AdminAuditLogRepository(IdentityDbContext dbContext) : IAdminAuditLogRepository
{
    public void Add(AdminAuditLogEntry entry)
    {
        dbContext.AdminAuditLogEntries.Add(entry);
    }
}
