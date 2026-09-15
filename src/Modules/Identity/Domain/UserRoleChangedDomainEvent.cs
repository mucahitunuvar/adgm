using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Identity.Domain;

public sealed record UserRoleChangedDomainEvent(Guid UserId, UserRole PreviousRole, UserRole NewRole) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
