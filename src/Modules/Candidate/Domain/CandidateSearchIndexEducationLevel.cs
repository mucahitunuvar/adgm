using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Candidate.Domain;

// Junction row for CandidateSearchIndex.EducationLevelIds (ADR-020) - one row per distinct
// EducationLevelId found across the candidate's CandidateCvContent.Educations. Lets Görev 2's
// listing filter (`educationLevelId`) use ordinary, reliably-translated LINQ instead of depending on
// JSON-column query translation.
public sealed class CandidateSearchIndexEducationLevel : Entity
{
    public Guid CandidateSearchIndexId { get; private set; }

    public Guid EducationLevelId { get; private set; }

    private CandidateSearchIndexEducationLevel(Guid id, Guid candidateSearchIndexId, Guid educationLevelId)
        : base(id)
    {
        CandidateSearchIndexId = candidateSearchIndexId;
        EducationLevelId = educationLevelId;
    }

    internal static CandidateSearchIndexEducationLevel Create(Guid candidateSearchIndexId, Guid educationLevelId) =>
        new(Guid.NewGuid(), candidateSearchIndexId, educationLevelId);
}
