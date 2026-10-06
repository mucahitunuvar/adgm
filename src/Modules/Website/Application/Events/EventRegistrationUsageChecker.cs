using GenclikMerkezi.Modules.Website.Application.Abstractions;

namespace GenclikMerkezi.Modules.Website.Application.Events;

// ADR-024 §11.1/§11.2 (Faz 4 Görev 3): the real implementation behind IEventRegistrationUsageChecker,
// replacing Görev 1's NoOpEventRegistrationUsageChecker now that EventRegistration exists -
// DeleteEventScheduleCommandHandler's and ContentItemPermanentDeletionService's "a calendar/content
// item with registrations cannot be deleted" guard.
public sealed class EventRegistrationUsageChecker(IEventRegistrationRepository eventRegistrationRepository) : IEventRegistrationUsageChecker
{
    public Task<bool> HasRegistrationsAsync(Guid contentItemId, CancellationToken cancellationToken = default) =>
        eventRegistrationRepository.HasRegistrationsAsync(contentItemId, cancellationToken);
}
