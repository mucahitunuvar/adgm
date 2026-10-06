namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §11.1/§11.2 (Faz 4 Görev 1): DeleteEventScheduleCommandHandler's and
// ContentItemPermanentDeletionService's "a calendar/content item with registrations cannot be deleted"
// guard (Event.HasRegistrations). EventRegistration does not exist until Görev 3, so this Görev wires
// the port with a no-op implementation (NoOpEventRegistrationUsageChecker) that always returns false -
// nothing can have registrations yet, so nothing is blocked. Görev 3 replaces the implementation only.
public interface IEventRegistrationUsageChecker
{
    Task<bool> HasRegistrationsAsync(Guid contentItemId, CancellationToken cancellationToken = default);
}
