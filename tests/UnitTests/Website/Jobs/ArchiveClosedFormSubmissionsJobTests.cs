using GenclikMerkezi.Modules.Website;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Infrastructure.Jobs;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.UnitTests.Website.Jobs;

// Hangfire runtime devreye girmez - job kendi IServiceScopeFactory'sini bir ServiceCollection'dan
// çözer, bu yüzden sahte repository'ler buraya kaydedilir ve ExecuteAsync doğrudan çağrılır (aynı
// desen PermanentlyDeleteExpiredTrashJobTests'te kullanılıyor).
public class ArchiveClosedFormSubmissionsJobTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly DateTime Now = new(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeFormSubmissionRepository _formSubmissionRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private ArchiveClosedFormSubmissionsJob CreateJob(DateTime now)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IFormSubmissionRepository>(_formSubmissionRepository);
        services.AddKeyedSingleton<IUnitOfWork>(WebsiteModuleMarker.UnitOfWorkKey, _unitOfWork);
        var serviceProvider = services.BuildServiceProvider();

        return new ArchiveClosedFormSubmissionsJob(serviceProvider.GetRequiredService<IServiceScopeFactory>(), new FixedTimeProvider(now));
    }

    private static FormSubmission CreateClosedSubmission(DateTime closedAtUtc)
    {
        var submission = FormSubmission.Create(
            Guid.NewGuid(), Guid.NewGuid(), 1, "[]", $"GM-2026-{Random.Shared.Next(100000, 999999)}", Tr, closedAtUtc, Guid.NewGuid(), null,
            "{}", [], []).Value;
        submission.ChangeStatus(FormSubmissionStatus.InReview, Guid.NewGuid(), closedAtUtc);
        submission.ChangeStatus(FormSubmissionStatus.Rejected, Guid.NewGuid(), closedAtUtc);
        return submission;
    }

    [Fact]
    public async Task ExecuteAsync_ArchivesSubmissionClosedMoreThan30DaysAgo()
    {
        var submission = CreateClosedSubmission(Now.AddDays(-31));
        _formSubmissionRepository.Seed(submission);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.NotNull(submission.ArchivedAtUtc);
    }

    [Fact]
    public async Task ExecuteAsync_LeavesSubmissionClosed29DaysAgoUntouched()
    {
        var submission = CreateClosedSubmission(Now.AddDays(-29));
        _formSubmissionRepository.Seed(submission);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.Null(submission.ArchivedAtUtc);
    }

    [Fact]
    public async Task ExecuteAsync_NeverArchivesAnOpenSubmission()
    {
        var submission = FormSubmission.Create(
            Guid.NewGuid(), Guid.NewGuid(), 1, "[]", "GM-2026-000001", Tr, Now.AddDays(-60), Guid.NewGuid(), null, "{}", [], []).Value;
        _formSubmissionRepository.Seed(submission);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.Null(submission.ArchivedAtUtc);
    }

    [Fact]
    public async Task ExecuteAsync_AfterUnarchive_RecountsFrom30DaysSinceUnarchive()
    {
        var submission = CreateClosedSubmission(Now.AddDays(-60));
        submission.Archive(Now.AddDays(-40));
        submission.Unarchive(Now.AddDays(-10));
        _formSubmissionRepository.Seed(submission);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        // Only 10 days have passed since the unarchive moment, not 60 since the original closing.
        Assert.Null(submission.ArchivedAtUtc);
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }
}
