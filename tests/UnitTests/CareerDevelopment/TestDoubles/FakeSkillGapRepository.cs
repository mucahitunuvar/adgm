using GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;
using GenclikMerkezi.Modules.CareerDevelopment.Domain;

namespace GenclikMerkezi.UnitTests.CareerDevelopment.TestDoubles;

public sealed class FakeSkillGapRepository : ISkillGapRepository
{
    private readonly List<SkillGap> _skillGaps = [];

    public IReadOnlyCollection<SkillGap> SkillGaps => _skillGaps.AsReadOnly();

    public Task<SkillGap?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_skillGaps.FirstOrDefault(s => s.Id == id));

    public Task<IReadOnlyList<SkillGap>> GetByCandidateCvIdAsync(Guid candidateCvId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SkillGap> matches = _skillGaps.Where(s => s.CandidateCvId == candidateCvId).ToList();
        return Task.FromResult(matches);
    }

    public void Add(SkillGap skillGap)
    {
        _skillGaps.Add(skillGap);
    }
}
