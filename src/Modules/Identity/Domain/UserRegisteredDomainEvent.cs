using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Identity.Domain;

public sealed record UserRegisteredDomainEvent(Guid UserId, string Email, UserRole Role) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
