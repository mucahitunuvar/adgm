using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Candidate.Domain;

// Raised whenever CandidateCv's own state changes, so the completion-percentage read-model
// (Candidate module design ADR, Decision 3) can recalculate. In-process MediatR dispatch only - no
// Outbox/RabbitMQ: CAP supports exactly one instance per process and is already anchored to
// IdentityDbContext (ADR-014's amendment), so Candidate has no transactional-outbox path of its own,
// and this event's producer and consumer are the same module/process/database anyway - the
// durability a message broker buys does not apply here.
public sealed record CandidateCvUpdatedDomainEvent(Guid CandidateCvId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
