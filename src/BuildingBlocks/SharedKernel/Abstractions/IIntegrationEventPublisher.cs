namespace GenclikMerkezi.SharedKernel.Abstractions;

public interface IIntegrationEventPublisher
{
    Task PublishTransactionalAsync<TEvent>(
        string topic,
        TEvent integrationEvent,
        Func<Task> persistChangesAsync,
        CancellationToken cancellationToken = default);
}
