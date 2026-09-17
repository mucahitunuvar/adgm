using GenclikMerkezi.Contracts.ReferenceData;

namespace GenclikMerkezi.Modules.ReferenceData.Domain;

// ADMIN-MANAGED: structure only, deliberately left empty of seed data (task asks to populate this
// once the Candidate module exists) - see ADR-008's own distinction between ReferenceData.Skill
// (this - the standard skill definition) and Candidate.CandidateSkill (a candidate's proficiency
// in one of these).
public sealed class Skill : LookupItem, ILookupItemFactory<Skill>
{
    private Skill(Guid id, string code, string displayName, int sortOrder)
        : base(id, code, displayName, sortOrder)
    {
    }

    private Skill()
    {
    }

    public static ReferenceDataLookupType LookupType => ReferenceDataLookupType.Skill;

    public static Skill Create(string code, string displayName, int sortOrder) =>
        new(Guid.NewGuid(), code, displayName, sortOrder);
}
