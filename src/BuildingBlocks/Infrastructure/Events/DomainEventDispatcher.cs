using GenclikMerkezi.SharedKernel.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace GenclikMerkezi.BuildingBlocks.Infrastructure.Events;

public static class DomainEventDispatcher
{
    public static async Task DispatchAndClearEventsAsync(
        IEnumerable<EntityEntry<AggregateRoot>> aggregateRoots,
        IPublisher publisher,
        CancellationToken cancellationToken = default)
    {
        var aggregates = aggregateRoots.Select(entry => entry.Entity).ToList();
        var domainEvents = aggregates.SelectMany(a => a.DomainEvents).ToList();

        foreach (var aggregate in aggregates)
        {
            aggregate.ClearDomainEvents();
        }

        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
            await publisher.Publish(notification, cancellationToken);
        }
    }
}
