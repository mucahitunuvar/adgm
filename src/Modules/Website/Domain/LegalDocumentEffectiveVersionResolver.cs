namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.1: single source of truth for "yürürlükteki sürüm" - the highest-numbered Published or
// Superseded version whose EffectiveAtUtc has already passed. A Draft is never effective, and a
// future-dated EffectiveAtUtc does not yet replace whichever earlier (Superseded) version still
// qualifies - exactly the "tek bir expression" §12.1 asks for, the same single-source-of-truth
// rationale SlideVisibility documents for its own time-window check.
public static class LegalDocumentEffectiveVersionResolver
{
    public static LegalDocumentVersion? Resolve(IReadOnlyList<LegalDocumentVersion> versions, DateTime now) =>
        versions
            .Where(v => v.Status != LegalDocumentVersionStatus.Draft && v.EffectiveAtUtc is not null && v.EffectiveAtUtc <= now)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefault();
}
