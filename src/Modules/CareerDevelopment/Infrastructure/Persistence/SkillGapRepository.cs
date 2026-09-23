using GenclikMerkezi.Modules.CareerDevelopment.Application.Abstractions;
using GenclikMerkezi.Modules.CareerDevelopment.Domain;
using Microsoft.EntityFrameworkCore;

namespace GenclikMerkezi.Modules.CareerDevelopment.Infrastructure.Persistence;

public sealed class SkillGapRepository(CareerDevelopmentDbContext dbContext) : ISkillGapRepository
{
    public Task<SkillGap?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.SkillGaps.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<SkillGap>> GetByCandidateCvIdAsync(Guid candidateCvId, CancellationToken cancellationToken = default)
    {
        return await dbContext.SkillGaps
            .AsNoTracking()
            .Where(s => s.CandidateCvId == candidateCvId)
            .ToListAsync(cancellationToken);
    }

    public void Add(SkillGap skillGap)
    {
        dbContext.SkillGaps.Add(skillGap);
    }
}
