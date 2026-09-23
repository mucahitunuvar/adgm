using GenclikMerkezi.Modules.CareerDevelopment.Domain;

namespace GenclikMerkezi.Modules.CareerDevelopment.Features.GetCandidateDevelopmentSummary;

public sealed record SkillGapItemResponse(Guid Id, Guid SkillId, Guid IdentifiedByAdvisorId, string? Notes, DateTime IdentifiedAtUtc)
{
    public static SkillGapItemResponse FromDomain(SkillGap skillGap) =>
        new(skillGap.Id, skillGap.SkillId, skillGap.IdentifiedByAdvisorId, skillGap.Notes, skillGap.IdentifiedAtUtc);
}
