using System.Diagnostics;
using GenclikMerkezi.BuildingBlocks.Infrastructure.Events;
using GenclikMerkezi.Modules.Identity;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Identity.Features.AdminDeactivateUser;

public sealed class UserDeactivatedAuditLogHandler(
    IAdminAuditLogRepository auditLogRepository,
    ICurrentUserService currentUserService,
    [FromKeyedServices(IdentityModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : INotificationHandler<DomainEventNotification<UserDeactivatedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<UserDeactivatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var entry = AdminAuditLogEntry.Create(
            currentUserService.UserId ?? Guid.Empty,
            AdminActionType.Deactivated,
            notification.DomainEvent.UserId,
            correlationId: Activity.Current?.Id);

        auditLogRepository.Add(entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
