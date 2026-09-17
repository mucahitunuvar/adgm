using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Candidate.Domain;

// Raised whenever CandidateCvContent's own state changes, so the completion-percentage read-model
// (Candidate module design ADR, Decision 3) can recalculate. See CandidateCvUpdatedDomainEvent for
// why this stays in-process (MediatR) rather than going through Outbox/RabbitMQ.
public sealed record CandidateCvContentUpdatedDomainEvent(Guid CandidateCvContentId, Guid CandidateCvId) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
