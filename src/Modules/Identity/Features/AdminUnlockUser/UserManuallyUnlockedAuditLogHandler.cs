using System.Diagnostics;
using GenclikMerkezi.BuildingBlocks.Infrastructure.Events;
using GenclikMerkezi.Modules.Identity;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Identity.Features.AdminUnlockUser;

public sealed class UserManuallyUnlockedAuditLogHandler(
    IAdminAuditLogRepository auditLogRepository,
    ICurrentUserService currentUserService,
    [FromKeyedServices(IdentityModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : INotificationHandler<DomainEventNotification<UserManuallyUnlockedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<UserManuallyUnlockedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var entry = AdminAuditLogEntry.Create(
            currentUserService.UserId ?? Guid.Empty,
            AdminActionType.ManuallyUnlocked,
            notification.DomainEvent.UserId,
            correlationId: Activity.Current?.Id);

        auditLogRepository.Add(entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
