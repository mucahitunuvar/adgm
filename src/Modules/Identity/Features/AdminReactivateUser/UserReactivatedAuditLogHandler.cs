using System.Diagnostics;
using GenclikMerkezi.BuildingBlocks.Infrastructure.Events;
using GenclikMerkezi.Modules.Identity;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Identity.Features.AdminReactivateUser;

public sealed class UserReactivatedAuditLogHandler(
    IAdminAuditLogRepository auditLogRepository,
    ICurrentUserService currentUserService,
    [FromKeyedServices(IdentityModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : INotificationHandler<DomainEventNotification<UserReactivatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<UserReactivatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var entry = AdminAuditLogEntry.Create(
            currentUserService.UserId ?? Guid.Empty,
            AdminActionType.Reactivated,
            notification.DomainEvent.UserId,
            correlationId: Activity.Current?.Id);

        auditLogRepository.Add(entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
