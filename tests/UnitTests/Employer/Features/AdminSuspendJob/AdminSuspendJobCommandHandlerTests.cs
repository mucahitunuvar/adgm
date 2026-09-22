using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.AdminSuspendJob;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.AdminSuspendJob;

public class AdminSuspendJobCommandHandlerTests
{
    private readonly FakeJobRepository _jobRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly Guid _adminUserId = Guid.NewGuid();

    private AdminSuspendJobCommandHandler CreateHandler() =>
        new(_jobRepository, new FakeCurrentUserContext(_adminUserId), _unitOfWork);

    private static Job CreatePublishedJob()
    {
        var job = Job.Create(
            Guid.NewGuid(), "Kaynakçı", false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "<p>Açıklama</p>", Guid.NewGuid(), [], [], [], [], [], DateTime.UtcNow);
        job.Submit();
        job.Approve(Guid.NewGuid(), DateTime.UtcNow);
        return job;
    }

    [Fact]
    public async Task Handle_WithPublishedJob_SuspendsAndSaves()
    {
        var job = CreatePublishedJob();
        _jobRepository.Add(job);

        var result = await CreateHandler().Handle(new AdminSuspendJobCommand(job.Id, "Uygunsuz içerik"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(JobStatus.SuspendedByAdmin, job.Status);
        Assert.Equal(_adminUserId, job.SuspendedByUserId);
        Assert.Equal("Uygunsuz içerik", job.SuspensionReason);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNotFound_AndDoesNotSave()
    {
        var result = await CreateHandler().Handle(new AdminSuspendJobCommand(Guid.NewGuid(), "Uygunsuz içerik"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithDraftJob_ReturnsConflict_AndDoesNotSave()
    {
        var job = Job.Create(
            Guid.NewGuid(), "Kaynakçı", false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "<p>Açıklama</p>", Guid.NewGuid(), [], [], [], [], [], DateTime.UtcNow);
        _jobRepository.Add(job);

        var result = await CreateHandler().Handle(new AdminSuspendJobCommand(job.Id, "Uygunsuz içerik"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
