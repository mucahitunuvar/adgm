using DotNetCore.CAP;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.BuildingBlocks.Infrastructure.Messaging;

// Wraps CAP's transactional-outbox publish pattern (BeginTransaction(capPublisher, autoCommit: true)
// + Publish) so Application-layer handlers depend only on IIntegrationEventPublisher, never on
// ICapPublisher/DbContext directly (AGENTS.md §7/§18). One instance per producing module's own
// DbContext keeps the outbox table inside that module's own database (AGENTS.md §9).
public sealed class CapIntegrationEventPublisher<TDbContext>(TDbContext dbContext, ICapPublisher capPublisher)
    : IIntegrationEventPublisher
    where TDbContext : DbContext
{
    public async Task PublishTransactionalAsync<TEvent>(
        string topic,
        TEvent integrationEvent,
        Func<Task> persistChangesAsync,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(capPublisher, autoCommit: true, cancellationToken);

        await persistChangesAsync();
        await capPublisher.PublishAsync(topic, integrationEvent, cancellationToken: cancellationToken);
    }
}
