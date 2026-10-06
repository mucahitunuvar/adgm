namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §11.1/§11.2 (Faz 4 Görev 1, real implementation in Görev 3): DeleteEventScheduleCommandHandler's
// and ContentItemPermanentDeletionService's "a calendar/content item with registrations cannot be
// deleted" guard (Event.HasRegistrations). Backed by EventRegistrationUsageChecker, which checks for
// any EventRegistration row at all (any status) via IEventRegistrationRepository.HasRegistrationsAsync.
public interface IEventRegistrationUsageChecker
{
    Task<bool> HasRegistrationsAsync(Guid contentItemId, CancellationToken cancellationToken = default);
}
