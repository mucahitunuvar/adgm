using GenclikMerkezi.Modules.Candidate.Domain;

namespace GenclikMerkezi.UnitTests.Candidate.Domain;

public class CareerAdvisorAssignmentSelectorTests
{
    [Fact]
    public void SelectLeastLoaded_WithNoActiveAdvisors_ReturnsNull()
    {
        var result = CareerAdvisorAssignmentSelector.SelectLeastLoaded([], new Dictionary<Guid, int>());

        Assert.Null(result);
    }

    [Fact]
    public void SelectLeastLoaded_WithSingleAdvisor_ReturnsIt()
    {
        var advisorId = Guid.NewGuid();

        var result = CareerAdvisorAssignmentSelector.SelectLeastLoaded([advisorId], new Dictionary<Guid, int>());

        Assert.Equal(advisorId, result);
    }

    [Fact]
    public void SelectLeastLoaded_WithDifferentWorkloads_ReturnsTheLeastLoadedAdvisor()
    {
        var busyAdvisorId = Guid.NewGuid();
        var idleAdvisorId = Guid.NewGuid();
        var workloadCounts = new Dictionary<Guid, int> { [busyAdvisorId] = 5, [idleAdvisorId] = 1 };

        var result = CareerAdvisorAssignmentSelector.SelectLeastLoaded([busyAdvisorId, idleAdvisorId], workloadCounts);

        Assert.Equal(idleAdvisorId, result);
    }

    [Fact]
    public void SelectLeastLoaded_WithAdvisorMissingFromWorkloadCounts_TreatsItAsZeroLoad()
    {
        var busyAdvisorId = Guid.NewGuid();
        var neverAssignedAdvisorId = Guid.NewGuid();
        var workloadCounts = new Dictionary<Guid, int> { [busyAdvisorId] = 1 };

        var result = CareerAdvisorAssignmentSelector.SelectLeastLoaded(
            [busyAdvisorId, neverAssignedAdvisorId], workloadCounts);

        Assert.Equal(neverAssignedAdvisorId, result);
    }
}
