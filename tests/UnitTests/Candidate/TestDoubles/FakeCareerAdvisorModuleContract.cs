using GenclikMerkezi.Contracts.CareerAdvisor;

namespace GenclikMerkezi.UnitTests.Candidate.TestDoubles;

public sealed class FakeCareerAdvisorModuleContract : ICareerAdvisorModuleContract
{
    public IReadOnlyList<ActiveCareerAdvisorSummary> ActiveAdvisors { get; set; } = [];

    public Task<IReadOnlyList<ActiveCareerAdvisorSummary>> GetActiveAdvisorsAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(ActiveAdvisors);
}
