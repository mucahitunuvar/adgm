using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Identity.Domain;

public sealed record UserLockedOutDomainEvent(Guid UserId, DateTime LockedUntilUtc) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
