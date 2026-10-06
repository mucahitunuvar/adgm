using GenclikMerkezi.Modules.Website.Application.Abstractions;

namespace GenclikMerkezi.Modules.Website.Application.Events;

// ADR-024 §11.1/§11.2 (Faz 4 Görev 1): placeholder implementation - see
// IEventRegistrationUsageChecker's own remarks. Görev 3 replaces this registration with the real
// implementation, backed by IEventRegistrationRepository.
public sealed class NoOpEventRegistrationUsageChecker : IEventRegistrationUsageChecker
{
    public Task<bool> HasRegistrationsAsync(Guid contentItemId, CancellationToken cancellationToken = default) => Task.FromResult(false);
}
