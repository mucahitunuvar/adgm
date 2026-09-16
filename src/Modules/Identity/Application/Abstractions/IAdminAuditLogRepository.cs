using GenclikMerkezi.Modules.Identity.Domain;

namespace GenclikMerkezi.Modules.Identity.Application.Abstractions;

public interface IAdminAuditLogRepository
{
    void Add(AdminAuditLogEntry entry);
}
