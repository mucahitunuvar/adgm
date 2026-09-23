using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.CareerDevelopment.Domain;

// Aggregate root (ADR-011). SkillGapId/CareerGoalId, aynı modül içi (doğrulanabilir - cross-module
// değil) isteğe bağlı referanslar; CreateDevelopmentPlanCommandHandler'da var olup olmadıkları ve
// aynı CandidateCvId'ye ait oldukları kontrol edilir. Interview.Schedule/Cancel'daki desenle aynı:
// Result döner, event yok.
public sealed class DevelopmentPlan : AggregateRoot
{
    public Guid CandidateCvId { get; private set; }

    public Guid? SkillGapId { get; private set; }

    public Guid? CareerGoalId { get; private set; }

    public string Description { get; private set; }

    public DevelopmentPlanStatus Status { get; private set; }

    public Guid CreatedByAdvisorId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    private DevelopmentPlan(
        Guid id, Guid candidateCvId, Guid? skillGapId, Guid? careerGoalId, string description,
        Guid createdByAdvisorId, DateTime createdAtUtc)
        : base(id)
    {
        CandidateCvId = candidateCvId;
        SkillGapId = skillGapId;
        CareerGoalId = careerGoalId;
        Description = description;
        Status = DevelopmentPlanStatus.Aktif;
        CreatedByAdvisorId = createdByAdvisorId;
        CreatedAtUtc = createdAtUtc;
    }

    public static DevelopmentPlan Create(
        Guid candidateCvId, Guid? skillGapId, Guid? careerGoalId, string description,
        Guid createdByAdvisorId, DateTime createdAtUtc) =>
        new(Guid.NewGuid(), candidateCvId, skillGapId, careerGoalId, description, createdByAdvisorId, createdAtUtc);

    public Result Complete(DateTime completedAtUtc)
    {
        if (Status != DevelopmentPlanStatus.Aktif)
        {
            return Result.Failure(Error.Conflict(
                "DevelopmentPlan.InvalidTransition", $"Cannot complete a development plan while status is {Status}."));
        }

        Status = DevelopmentPlanStatus.Tamamlandi;
        CompletedAtUtc = completedAtUtc;

        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status != DevelopmentPlanStatus.Aktif)
        {
            return Result.Failure(Error.Conflict(
                "DevelopmentPlan.InvalidTransition", $"Cannot cancel a development plan while status is {Status}."));
        }

        Status = DevelopmentPlanStatus.IptalEdildi;

        return Result.Success();
    }
}
