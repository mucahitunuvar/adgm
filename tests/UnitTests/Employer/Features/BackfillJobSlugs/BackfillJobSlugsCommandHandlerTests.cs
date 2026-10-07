using System.Reflection;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.BackfillJobSlugs;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.BackfillJobSlugs;

public class BackfillJobSlugsCommandHandlerTests
{
    private readonly FakeJobRepository _jobRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private BackfillJobSlugsCommandHandler CreateHandler() => new(_jobRepository, _unitOfWork);

    private static Job CreatePublishedJobWithoutSlug()
    {
        var job = Job.Create(
            Guid.NewGuid(), "Kaynakçı", false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "<p>Açıklama</p>", Guid.NewGuid(), [], [], [], [], [], DateTime.UtcNow);
        job.Submit();
        job.Approve(Guid.NewGuid(), DateTime.UtcNow);

        // Approve() artık Slug'ı her zaman atıyor - Slug sütunu eklenmeden önce yayınlanmış geçmiş
        // kayıtları simüle etmek için reflection ile temizleniyor (bkz. JobTests'teki aynı desen).
        typeof(Job).GetProperty(nameof(Job.Slug))!.SetValue(job, null);

        return job;
    }

    [Fact]
    public async Task Handle_WithJobsMissingSlug_AssignsSlugsAndSaves()
    {
        var job = CreatePublishedJobWithoutSlug();
        _jobRepository.Add(job);

        var result = await CreateHandler().Handle(new BackfillJobSlugsCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.UpdatedJobCount);
        Assert.Equal(JobSlugGenerator.Generate(job.Title, job.Id), job.Slug);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenNoJobsNeedBackfill_ReturnsZero_AndDoesNotSave()
    {
        var job = Job.Create(
            Guid.NewGuid(), "Kaynakçı", false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "<p>Açıklama</p>", Guid.NewGuid(), [], [], [], [], [], DateTime.UtcNow);
        _jobRepository.Add(job);

        var result = await CreateHandler().Handle(new BackfillJobSlugsCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value.UpdatedJobCount);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_CalledTwice_IsIdempotent()
    {
        var job = CreatePublishedJobWithoutSlug();
        _jobRepository.Add(job);
        var handler = CreateHandler();

        var firstResult = await handler.Handle(new BackfillJobSlugsCommand(), CancellationToken.None);
        var slugAfterFirstRun = job.Slug;
        var secondResult = await handler.Handle(new BackfillJobSlugsCommand(), CancellationToken.None);

        Assert.Equal(1, firstResult.Value.UpdatedJobCount);
        Assert.Equal(0, secondResult.Value.UpdatedJobCount);
        Assert.Equal(slugAfterFirstRun, job.Slug);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }
}
