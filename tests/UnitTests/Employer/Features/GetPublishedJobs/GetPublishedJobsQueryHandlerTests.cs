using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.GetPublishedJobs;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.GetPublishedJobs;

public class GetPublishedJobsQueryHandlerTests
{
    private readonly FakeJobRepository _jobRepository = new();

    private GetPublishedJobsQueryHandler CreateHandler() => new(_jobRepository);

    private static Job CreateJob(Guid companyId) =>
        Job.Create(
            companyId, "Kaynakçı", false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), null, Guid.NewGuid(), [], [], [], [], [], DateTime.UtcNow);

    [Fact]
    public async Task Handle_ReturnsOnlyPublishedJobs()
    {
        var draftJob = CreateJob(Guid.NewGuid());
        _jobRepository.Add(draftJob);

        var publishedJob = CreateJob(Guid.NewGuid());
        publishedJob.Submit();
        publishedJob.Approve(Guid.NewGuid(), DateTime.UtcNow);
        _jobRepository.Add(publishedJob);

        var rejectedJob = CreateJob(Guid.NewGuid());
        rejectedJob.Submit();
        rejectedJob.Reject("Eksik bilgi", DateTime.UtcNow);
        _jobRepository.Add(rejectedJob);

        var result = await CreateHandler().Handle(new GetPublishedJobsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var returnedJob = Assert.Single(result.Value);
        Assert.Equal(publishedJob.Id, returnedJob.Id);
        Assert.Equal(JobStatus.Published, returnedJob.Status);
    }
}
