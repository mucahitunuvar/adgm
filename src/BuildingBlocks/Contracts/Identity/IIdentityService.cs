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
}
