namespace GenclikMerkezi.Modules.Website.Features.GetPublicEvents;

// ADR-024 §17 (Faz 4 Görev 2): RegistrationState/RemainingSpots are always computed fresh (§1
// "kontenjan/durum alanları cache'lenmez") - never read back from the cached candidate page itself.
public sealed record PublicEventSummaryResponse(
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    string Format,
    string VenueName,
    bool IsCancelled,
    string RegistrationState,
    int? RemainingSpots);
