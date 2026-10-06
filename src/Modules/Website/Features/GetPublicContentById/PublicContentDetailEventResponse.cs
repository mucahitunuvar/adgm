namespace GenclikMerkezi.Modules.Website.Features.GetPublicContentById;

// ADR-024 §11/§17 (Faz 4 Görev 2): the detail page's event block - the summary fields plus the extra
// informational text/age/window fields §1 lists for the detail response. OnlineLink never appears
// here (§1 "OnlineLink hiçbir public yanıtta yer almaz"). RegistrationState/RemainingSpots are filled
// in by the handler after the cached response is read, never cached themselves.
public sealed record PublicContentDetailEventResponse(
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    string Format,
    string VenueName,
    bool IsCancelled,
    string RegistrationState,
    int? RemainingSpots,
    string VenueAddress,
    string FeeInfo,
    string Instructors,
    string ProgramFlow,
    string AccessibilityNote,
    int? MinAge,
    int? MaxAge,
    DateTime? RegistrationOpensAtUtc,
    DateTime? RegistrationClosesAtUtc);
