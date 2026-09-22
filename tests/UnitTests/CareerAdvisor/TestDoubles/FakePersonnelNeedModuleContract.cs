using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;

public sealed class FakePersonnelNeedModuleContract : IPersonnelNeedModuleContract
{
    private readonly Dictionary<Guid, PersonnelNeedSummary> _all = [];
    private readonly HashSet<Guid> _generalPoolIds = [];

    public Result CloseResult { get; set; } = Result.Success();

    public (Guid PersonnelNeedId, Guid ClosedByAdvisorId, Guid? FulfilledByCandidateCvId)? CloseCall { get; private set; }

    public void SeedGeneralPool(PersonnelNeedSummary personnelNeed)
    {
        _all[personnelNeed.Id] = personnelNeed;
        _generalPoolIds.Add(personnelNeed.Id);
    }

    // Matching Görev 7: havuzda OLMAYAN (veya zaten kapanmış) bir ihtiyacı GetByIdAsync/
    // IsInGeneralPoolAsync üzerinden test edebilmek için - SeedGeneralPool'un aksine _generalPoolIds'e
    // eklemez.
    public void Seed(PersonnelNeedSummary personnelNeed) => _all[personnelNeed.Id] = personnelNeed;

    public Task<PagedResult<PersonnelNeedSummary>> GetGeneralPoolAsync(
        PagedRequest request, CancellationToken cancellationToken = default)
    {
        var pool = _all.Values.Where(p => _generalPoolIds.Contains(p.Id)).ToList();
        var page = pool
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return Task.FromResult(new PagedResult<PersonnelNeedSummary>(page, pool.Count, request.Page, request.PageSize));
    }

    public Task<PersonnelNeedSummary?> GetByIdAsync(Guid personnelNeedId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_all.TryGetValue(personnelNeedId, out var summary) ? summary : null);

    public Task<bool> IsInGeneralPoolAsync(Guid personnelNeedId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_generalPoolIds.Contains(personnelNeedId));

    public Task<Result> CloseAsync(
        Guid personnelNeedId, Guid closedByAdvisorId, Guid? fulfilledByCandidateCvId, CancellationToken cancellationToken = default)
    {
        CloseCall = (personnelNeedId, closedByAdvisorId, fulfilledByCandidateCvId);
        return Task.FromResult(CloseResult);
    }
}
