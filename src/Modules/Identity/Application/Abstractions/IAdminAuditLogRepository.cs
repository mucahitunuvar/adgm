using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Identity.Application.Abstractions;

public interface IAdminAuditLogRepository
{
    void Add(AdminAuditLogEntry entry);

    Task<PagedResult<AdminAuditLogEntry>> SearchAsync(
        AdminAuditLogFilter filter,
        CancellationToken cancellationToken = default);
}
