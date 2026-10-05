using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

public interface IFormSubmissionSequenceRepository
{
    Task<FormSubmissionSequence?> GetByYearAsync(int year, CancellationToken cancellationToken = default);

    void Add(FormSubmissionSequence sequence);

    // Mirrors IFormSubmissionRepository.DetachFailedAdd - called when a concurrent reservation for the
    // same year wins the race, before the handler reloads and retries.
    void DetachFailedReservation(FormSubmissionSequence sequence);
}
