using GenclikMerkezi.SharedKernel.Domain;
using MediatR;

namespace GenclikMerkezi.BuildingBlocks.Infrastructure.Events;

public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
    where TDomainEvent : IDomainEvent;
