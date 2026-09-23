using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.CareerDevelopment.Domain;

// Aggregate root (ADR-011). TargetPositionId, ReferenceData.Position'a düz, isteğe bağlı bir referans
// (Job.PositionId gibi, FK yok). SkillGap.cs'teki desenle aynı - yalnızca kayıt, event yok.
public sealed class CareerGoal : AggregateRoot
{
    public Guid CandidateCvId { get; private set; }

    public string Description { get; private set; }

    public Guid? TargetPositionId { get; private set; }

    public Guid SetByAdvisorId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private CareerGoal(
        Guid id, Guid candidateCvId, string description, Guid? targetPositionId, Guid setByAdvisorId, DateTime createdAtUtc)
        : base(id)
    {
        CandidateCvId = candidateCvId;
        Description = description;
        TargetPositionId = targetPositionId;
        SetByAdvisorId = setByAdvisorId;
        CreatedAtUtc = createdAtUtc;
    }

    public static CareerGoal Create(
        Guid candidateCvId, string description, Guid? targetPositionId, Guid setByAdvisorId, DateTime createdAtUtc) =>
        new(Guid.NewGuid(), candidateCvId, description, targetPositionId, setByAdvisorId, createdAtUtc);
}
