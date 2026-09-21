using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;

public sealed class FakePersonnelNeedModuleContract : IPersonnelNeedModuleContract
{
    private readonly List<PersonnelNeedSummary> _generalPool = [];

    public Result CloseResult { get; set; } = Result.Success();

    public void SeedGeneralPool(PersonnelNeedSummary personnelNeed) => _generalPool.Add(personnelNeed);

    public Task<PagedResult<PersonnelNeedSummary>> GetGeneralPoolAsync(
        PagedRequest request, CancellationToken cancellationToken = default)
    {
        var page = _generalPool
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return Task.FromResult(new PagedResult<PersonnelNeedSummary>(page, _generalPool.Count, request.Page, request.PageSize));
    }

    public Task<Result> CloseAsync(
        Guid personnelNeedId, Guid closedByAdvisorId, Guid? fulfilledByCandidateCvId, CancellationToken cancellationToken = default) =>
        Task.FromResult(CloseResult);
}
