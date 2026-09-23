using GenclikMerkezi.Modules.CareerDevelopment.Domain;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.GetCandidateDevelopmentSummary;

// Status string olarak döner (InterviewResponse/EmploymentResponse ile aynı gerekçe - System.Text.Json
// bu projede JsonStringEnumConverter yapılandırılmadan sayı döner).
public sealed record DevelopmentPlanItemResponse(
    Guid Id,
    Guid? SkillGapId,
    Guid? CareerGoalId,
    string Description,
    string Status,
    Guid CreatedByAdvisorId,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc)
{
    public static DevelopmentPlanItemResponse FromDomain(DevelopmentPlan plan) => new(
        plan.Id, plan.SkillGapId, plan.CareerGoalId, plan.Description, plan.Status.ToString(),
        plan.CreatedByAdvisorId, plan.CreatedAtUtc, plan.CompletedAtUtc);
}
