using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Candidate.Domain;

// Junction row for CandidateSearchIndex.SectorIds (ADR-020) - one row per distinct SectorId found
// across the candidate's CandidateCvContent.Experiences. Lets Görev 2's listing filter (`sectorId`)
// use ordinary, reliably-translated LINQ instead of depending on JSON-column query translation.
public sealed class CandidateSearchIndexSector : Entity
{
    public Guid CandidateSearchIndexId { get; private set; }

    public Guid SectorId { get; private set; }

    private CandidateSearchIndexSector(Guid id, Guid candidateSearchIndexId, Guid sectorId)
        : base(id)
    {
        CandidateSearchIndexId = candidateSearchIndexId;
        SectorId = sectorId;
    }

    internal static CandidateSearchIndexSector Create(Guid candidateSearchIndexId, Guid sectorId) =>
        new(Guid.NewGuid(), candidateSearchIndexId, sectorId);
}
