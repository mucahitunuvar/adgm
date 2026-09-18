namespace GenclikMerkezi.Modules.Candidate.Domain;

// Domain service (AGENTS.md §10): pure projection from the two source aggregates into
// CandidateSearchIndex's denormalized shape (ADR-020). Stateless, no infrastructure dependency -
// mirrors CandidateCvCompletionCalculator. Callers are expected to have already recalculated
// CandidateCv.CompletionPercentage (via CandidateCvCompletionCalculator) before calling this, so the
// index reflects the up-to-date percentage rather than a stale one.
public static class CandidateSearchIndexProjector
{
    // Called once, at registration (RegisterCandidateCommand): a freshly created CandidateCv has no
    // CandidateCvContent-derived data yet and CandidateCv.Create itself raises no domain event, so
    // without this the candidate would have no CandidateSearchIndex row - and would not appear in the
    // admin/advisor listing (Görev 2) - until their first profile edit.
    public static CandidateSearchIndex CreateInitial(CandidateCv candidateCv, DateTime updatedAtUtc) =>
        CandidateSearchIndex.Create(
            candidateCv.Id,
            TurkishTextNormalizer.Normalize($"{candidateCv.FirstName} {candidateCv.LastName}"),
            candidateCv.Email,
            candidateCv.ProvinceId,
            candidateCv.DistrictId,
            candidateCv.CompletionPercentage,
            educationLevelIds: [],
            sectorIds: [],
            updatedAtUtc);

    public static void Project(
        CandidateSearchIndex index, CandidateCv candidateCv, CandidateCvContent? candidateCvContent, DateTime updatedAtUtc)
    {
        var fullNameNormalized = TurkishTextNormalizer.Normalize($"{candidateCv.FirstName} {candidateCv.LastName}");

        var educationLevelIds = candidateCvContent?.Educations.Select(e => e.EducationLevelId) ?? [];
        var sectorIds = candidateCvContent?.Experiences
            .Where(e => e.SectorId is not null)
            .Select(e => e.SectorId!.Value) ?? [];

        index.Refresh(
            fullNameNormalized,
            candidateCv.Email,
            candidateCv.ProvinceId,
            candidateCv.DistrictId,
            candidateCv.CompletionPercentage,
            educationLevelIds,
            sectorIds,
            updatedAtUtc);
    }
}
