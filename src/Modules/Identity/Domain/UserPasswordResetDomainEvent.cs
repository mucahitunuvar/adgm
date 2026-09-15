using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Identity.Domain;

public sealed record UserPasswordResetDomainEvent(Guid UserId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
