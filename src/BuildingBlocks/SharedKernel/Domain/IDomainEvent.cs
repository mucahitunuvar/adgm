namespace GenclikMerkezi.SharedKernel.Domain;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
