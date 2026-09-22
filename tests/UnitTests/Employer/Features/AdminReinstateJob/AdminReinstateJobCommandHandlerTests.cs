using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.AdminReinstateJob;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.AdminReinstateJob;

public class AdminReinstateJobCommandHandlerTests
{
    private readonly FakeJobRepository _jobRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private AdminReinstateJobCommandHandler CreateHandler() => new(_jobRepository, _unitOfWork);

    private static Job CreateSuspendedJob()
    {
        var job = Job.Create(
            Guid.NewGuid(), "Kaynakçı", false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "<p>Açıklama</p>", Guid.NewGuid(), [], [], [], [], [], DateTime.UtcNow);
        job.Submit();
        job.Approve(Guid.NewGuid(), DateTime.UtcNow);
        job.Suspend(Guid.NewGuid(), "Uygunsuz içerik", DateTime.UtcNow);
        return job;
    }

    [Fact]
    public async Task Handle_WithSuspendedJob_ReinstatesAndSaves()
    {
        var job = CreateSuspendedJob();
        _jobRepository.Add(job);

        var result = await CreateHandler().Handle(new AdminReinstateJobCommand(job.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(JobStatus.Published, job.Status);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNotFound_AndDoesNotSave()
    {
        var result = await CreateHandler().Handle(new AdminReinstateJobCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithPublishedJob_ReturnsConflict_AndDoesNotSave()
    {
        var job = Job.Create(
            Guid.NewGuid(), "Kaynakçı", false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "<p>Açıklama</p>", Guid.NewGuid(), [], [], [], [], [], DateTime.UtcNow);
        job.Submit();
        job.Approve(Guid.NewGuid(), DateTime.UtcNow);
        _jobRepository.Add(job);

        var result = await CreateHandler().Handle(new AdminReinstateJobCommand(job.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
