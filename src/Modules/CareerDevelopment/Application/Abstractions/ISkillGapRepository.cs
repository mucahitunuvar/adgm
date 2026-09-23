using GenclikMerkezi.Modules.CareerDevelopment.Domain;

namespace GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;

public interface ISkillGapRepository
{
    Task<SkillGap?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SkillGap>> GetByCandidateCvIdAsync(Guid candidateCvId, CancellationToken cancellationToken = default);

    void Add(SkillGap skillGap);
}
