using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.CareerDevelopment.Domain;

// Aggregate root (ADR-011). CandidateCvId, Candidate modülüne doğrulanmayan cross-module referans
// (CandidateNote.CandidateCvId ile aynı desen). SkillId, ReferenceData.Skill'e düz bir referans
// (Job.PositionId gibi, FK yok). Yalnızca kayıt tutar - event fırlatmaz, Create dışında domain
// metodu yok (durum makinesi DevelopmentPlan'da).
public sealed class SkillGap : AggregateRoot
{
    public Guid CandidateCvId { get; private set; }

    public Guid SkillId { get; private set; }

    public Guid IdentifiedByAdvisorId { get; private set; }

    public string? Notes { get; private set; }

    public DateTime IdentifiedAtUtc { get; private set; }

    private SkillGap(Guid id, Guid candidateCvId, Guid skillId, Guid identifiedByAdvisorId, string? notes, DateTime identifiedAtUtc)
        : base(id)
    {
        CandidateCvId = candidateCvId;
        SkillId = skillId;
        IdentifiedByAdvisorId = identifiedByAdvisorId;
        Notes = notes;
        IdentifiedAtUtc = identifiedAtUtc;
    }

    public static SkillGap Create(Guid candidateCvId, Guid skillId, Guid identifiedByAdvisorId, string? notes, DateTime identifiedAtUtc) =>
        new(Guid.NewGuid(), candidateCvId, skillId, identifiedByAdvisorId, notes, identifiedAtUtc);
}
