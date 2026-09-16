using System.Diagnostics;
using System.Text.Json;
using GenclikMerkezi.BuildingBlocks.Infrastructure.Events;
using GenclikMerkezi.Modules.Identity;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Identity.Features.ChangeUserRole;

// Reacts to the domain event ChangeUserRoleCommandHandler's User.ChangeRole already raises,
// rather than writing the audit entry inline in that handler (ADR-015): the command handler stays
// focused on the state change, and this is the one place a future admin action's audit trail gets
// added, without touching unrelated command handlers.
public sealed class UserRoleChangedAuditLogHandler(
    IAdminAuditLogRepository auditLogRepository,
    ICurrentUserService currentUserService,
    [FromKeyedServices(IdentityModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : INotificationHandler<DomainEventNotification<UserRoleChangedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<UserRoleChangedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var details = JsonSerializer.Serialize(new
        {
            PreviousRole = domainEvent.PreviousRole.ToString(),
            NewRole = domainEvent.NewRole.ToString(),
        });

        var entry = AdminAuditLogEntry.Create(
            currentUserService.UserId ?? Guid.Empty,
            AdminActionType.RoleChanged,
            domainEvent.UserId,
            details,
            Activity.Current?.Id);

        auditLogRepository.Add(entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
