using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Contracts.Identity;

// The published, in-process contract other modules depend on instead of Identity's own DbContext
// (ADR-016 Decision 2, Option C - the same in-process Contracts-interface pattern used for
// IReferenceDataLookupReader). Implemented in Identity.Infrastructure, registered once at the Host
// composition root, and injected directly - a plain in-process method call, not a network request.
// Candidate's registration orchestration (ADR-017 Decision 2) reads FirstName/LastName/PhoneNumber
// through this contract rather than reaching into Identity's database.
public interface IIdentityService
{
    Task<IdentityUserProfile?> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    // Drives Identity's own RegisterUserCommand (duplicate-email check, email-verification token,
    // UserRegisteredIntegrationEvent), so a consuming module's own registration orchestration
    // (e.g. Candidate's RegisterCandidateCommand) triggers the exact same account-creation/
    // email-verification flow as registering directly through Identity's endpoint, rather than a
    // second, divergent implementation of it.
    Task<Result<Guid>> CreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string? phoneNumber,
        string role,
        CancellationToken cancellationToken = default);

    // Compensating action for a saga-lite registration orchestration (e.g. ADR-017 Decision 2): when
    // a consuming module's own aggregate fails to persist after CreateUserAsync already committed,
    // the caller uses this to roll the just-created account back rather than leave an orphaned User
    // with no corresponding profile. Deliberately not AdminDeactivateUserCommand: this is a
    // system-triggered compensation with no admin actor in context, not an admin action, and must
    // not be attributed to one in the admin audit log.
    Task DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
