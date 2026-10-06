namespace GenclikMerkezi.Contracts.Website;

// ADR-024 §11.2 (Faz 4 Görev 3): a port Website defines for project-specific wiring done by a
// Host-level adapter, never by Website itself - the same split IWebsiteEmailSender's own remarks
// describe (Website "must stay portable and depend on no other module", enforced by
// WebsiteContractsBoundaryTests). CreateEventRegistrationCommandHandler needs to know whether a
// logged-in registrant's platform account already has a confirmed email (to skip the anonymous
// verification-email step) without Website depending on GenclikMerkezi.Contracts.Identity directly.
// This project's Host adapter forwards to IIdentityService.GetUserProfileAsync; a different project
// without an Identity module could implement this port however it tracks user accounts.
public interface IPlatformUserEmailConfirmationLookup
{
    // False for an unknown userId, not just an unconfirmed one - from Website's perspective both mean
    // "do not skip verification".
    Task<bool> IsEmailConfirmedAsync(Guid userId, CancellationToken cancellationToken = default);
}
