using GenclikMerkezi.SharedKernel.Abstractions;

namespace GenclikMerkezi.UnitTests.Identity.TestDoubles;

public sealed class FakeIntegrationEventPublisher : IIntegrationEventPublisher
{
    public List<(string Topic, object IntegrationEvent)> PublishedEvents { get; } = [];

    public async Task PublishTransactionalAsync<TEvent>(
        string topic,
        TEvent integrationEvent,
        Func<Task> persistChangesAsync,
        CancellationToken cancellationToken = default)
    {
        await persistChangesAsync();
        PublishedEvents.Add((topic, integrationEvent!));
    }
}
