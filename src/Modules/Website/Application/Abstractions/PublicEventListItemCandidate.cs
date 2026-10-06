using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §17 (Faz 4 Görev 2): GetPublicEventsQueryHandler's cached projection shape - display fields
// only. ConfirmedCount (and everything else EventRegistrationStateResolver needs) is deliberately
// excluded: §1 "kontenjan/durum alanları cache'lenmez" - EventRegistrationStateInputs is fetched
// separately, uncached, every request.
public sealed record PublicEventListItemCandidate(
    Guid ContentItemId,
    string Title,
    string FullPath,
    Guid? CoverImageMediaId,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    EventFormat Format,
    string VenueName,
    bool IsCancelled);
