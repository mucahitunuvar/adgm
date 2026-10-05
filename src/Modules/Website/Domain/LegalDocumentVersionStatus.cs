namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.1: a version's lifecycle - Draft (being authored, at most one per document), Published
// (the most recently published version, regardless of whether its EffectiveAtUtc has arrived yet) and
// Superseded (an earlier version a newer one has replaced). Which Published/Superseded version is
// actually in force right now is a separate question - see LegalDocumentEffectiveVersionResolver.
public enum LegalDocumentVersionStatus
{
    Draft,
    Published,
    Superseded,
}
