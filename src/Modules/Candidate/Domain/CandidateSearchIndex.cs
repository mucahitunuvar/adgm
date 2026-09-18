using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Candidate.Domain;

// Denormalized read-model for admin/advisor listing and filtering (ADR-020). Id is the owning
// CandidateCv's own Id (1-1) - not a separately generated key. Kept in sync, in-process and
// synchronously, by the same domain event dispatch chain that already recalculates
// CompletionPercentage (Görev 6/ADR-018's revision); never written to by any command handler
// directly. EducationLevelIds/SectorIds are backed by small junction tables (rather than a JSON
// column) so Görev 2's filters can use plain, reliably-translated LINQ (`Any(x => x.Id == filterId)`)
// instead of depending on JSON-column query translation.
public sealed class CandidateSearchIndex : Entity
{
    private readonly List<CandidateSearchIndexEducationLevel> _educationLevels = [];
    private readonly List<CandidateSearchIndexSector> _sectors = [];

    public string FullNameNormalized { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public Guid? ProvinceId { get; private set; }

    public Guid? DistrictId { get; private set; }

    public int CompletionPercentage { get; private set; }

    public IReadOnlyCollection<CandidateSearchIndexEducationLevel> EducationLevels => _educationLevels.AsReadOnly();

    public IReadOnlyCollection<Guid> EducationLevelIds => _educationLevels.Select(e => e.EducationLevelId).ToArray();

    public IReadOnlyCollection<CandidateSearchIndexSector> Sectors => _sectors.AsReadOnly();

    public IReadOnlyCollection<Guid> SectorIds => _sectors.Select(s => s.SectorId).ToArray();

    public DateTime UpdatedAtUtc { get; private set; }

    private CandidateSearchIndex(Guid candidateCvId)
        : base(candidateCvId)
    {
    }

    private CandidateSearchIndex()
    {
    }

    public static CandidateSearchIndex Create(
        Guid candidateCvId,
        string fullNameNormalized,
        string email,
        Guid? provinceId,
        Guid? districtId,
        int completionPercentage,
        IEnumerable<Guid> educationLevelIds,
        IEnumerable<Guid> sectorIds,
        DateTime updatedAtUtc)
    {
        var index = new CandidateSearchIndex(candidateCvId);
        index.Refresh(
            fullNameNormalized, email, provinceId, districtId, completionPercentage, educationLevelIds, sectorIds, updatedAtUtc);

        return index;
    }

    public void Refresh(
        string fullNameNormalized,
        string email,
        Guid? provinceId,
        Guid? districtId,
        int completionPercentage,
        IEnumerable<Guid> educationLevelIds,
        IEnumerable<Guid> sectorIds,
        DateTime updatedAtUtc)
    {
        FullNameNormalized = fullNameNormalized;
        Email = email;
        ProvinceId = provinceId;
        DistrictId = districtId;
        CompletionPercentage = completionPercentage;

        _educationLevels.Clear();
        _educationLevels.AddRange(educationLevelIds.Distinct().Select(id => CandidateSearchIndexEducationLevel.Create(Id, id)));

        _sectors.Clear();
        _sectors.AddRange(sectorIds.Distinct().Select(id => CandidateSearchIndexSector.Create(Id, id)));

        UpdatedAtUtc = updatedAtUtc;
    }
}
