using GenclikMerkezi.Modules.Website;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Infrastructure.Jobs;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.UnitTests.Website.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace GenclikMerkezi.UnitTests.Website.Jobs;

// Hangfire runtime devreye girmez - job kendi IServiceScopeFactory'sini bir ServiceCollection'dan
// çözer, bu yüzden sahte repository'ler buraya kaydedilir ve ExecuteAsync doğrudan çağrılır (aynı
// desen PermanentlyDeleteExpiredTrashJobTests'te kullanılıyor). FakeFormSubmissionRepository treats a
// submission as "due for anonymization" purely by SubmittedAtUtc <= now (see its own comment) - the
// real per-form RetentionDays join is covered by FormSubmissionFlowTests (integration).
public class AnonymizeExpiredFormSubmissionsJobTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly DateTime Now = new(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeFormSubmissionRepository _formSubmissionRepository = new();
    private readonly FakeFormSubmissionFileStorageService _fileStorageService = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private AnonymizeExpiredFormSubmissionsJob CreateJob(DateTime now)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IFormSubmissionRepository>(_formSubmissionRepository);
        services.AddSingleton<IFileStorageService>(_fileStorageService);
        services.AddKeyedSingleton<IUnitOfWork>(WebsiteModuleMarker.UnitOfWorkKey, _unitOfWork);
        var serviceProvider = services.BuildServiceProvider();

        return new AnonymizeExpiredFormSubmissionsJob(
            serviceProvider.GetRequiredService<IServiceScopeFactory>(), new FixedTimeProvider(now),
            NullLogger<AnonymizeExpiredFormSubmissionsJob>.Instance);
    }

    private static FormSubmission CreateSubmission(DateTime submittedAtUtc, FileAttachment? file = null)
    {
        IReadOnlyList<FormSubmissionFileAttachment> attachments = file is null ? [] : [FormSubmissionFileAttachment.Create("cv", file)];
        return FormSubmission.Create(
            Guid.NewGuid(), Guid.NewGuid(), 1, "[]", $"GM-2026-{Random.Shared.Next(100000, 999999)}", Tr, submittedAtUtc, Guid.NewGuid(), null,
            """{"email":"ali@example.com"}""", attachments, []).Value;
    }

    private static FileAttachment CreateFile(string key) =>
        FileAttachment.Create(key, "cv.pdf", "application/pdf", 1024, Now, "FormSubmission", Guid.NewGuid());

    [Fact]
    public async Task ExecuteAsync_AnonymizesDueSubmission_ClearingPersonalDataButKeepingReferenceAndDates()
    {
        var submission = CreateSubmission(Now.AddDays(-1));
        _formSubmissionRepository.Seed(submission);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.NotNull(submission.AnonymizedAtUtc);
        Assert.Equal("{}", submission.ResponsesJson);
        Assert.Null(submission.SubmittedByUserId);
        // Remaining fields untouched.
        Assert.StartsWith("GM-2026-", submission.ReferenceNumber);
        Assert.Equal(Now.AddDays(-1), submission.SubmittedAtUtc);
    }

    [Fact]
    public async Task ExecuteAsync_LeavesNotYetDueSubmissionUntouched()
    {
        var submission = CreateSubmission(Now.AddDays(1));
        _formSubmissionRepository.Seed(submission);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.Null(submission.AnonymizedAtUtc);
        Assert.NotEqual("{}", submission.ResponsesJson);
    }

    [Fact]
    public async Task ExecuteAsync_DeletesFileFromStorageAfterAnonymizing()
    {
        var file = CreateFile("website-form-attachments/2026/10/05/one.pdf");
        var submission = CreateSubmission(Now.AddDays(-1), file);
        _formSubmissionRepository.Seed(submission);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.Contains(file.FileKey, _fileStorageService.DeletedFileKeys);
        Assert.Empty(submission.PendingFileDeletions);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStorageDeleteFails_KeepsPendingDeletionForNextRun()
    {
        var file = CreateFile("website-form-attachments/2026/10/05/fails.pdf");
        _fileStorageService.FailDeleteFor(file.FileKey);
        var submission = CreateSubmission(Now.AddDays(-1), file);
        _formSubmissionRepository.Seed(submission);

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        Assert.DoesNotContain(file.FileKey, _fileStorageService.DeletedFileKeys);
        Assert.Single(submission.PendingFileDeletions);

        // Next run (failure no longer simulated) succeeds and clears the pending deletion.
        var fileStorageService2 = new FakeFormSubmissionFileStorageService();
        var services = new ServiceCollection();
        services.AddSingleton<IFormSubmissionRepository>(_formSubmissionRepository);
        services.AddSingleton<IFileStorageService>(fileStorageService2);
        services.AddKeyedSingleton<IUnitOfWork>(WebsiteModuleMarker.UnitOfWorkKey, _unitOfWork);
        var serviceProvider = services.BuildServiceProvider();
        var secondRun = new AnonymizeExpiredFormSubmissionsJob(
            serviceProvider.GetRequiredService<IServiceScopeFactory>(), new FixedTimeProvider(Now.AddDays(1)),
            NullLogger<AnonymizeExpiredFormSubmissionsJob>.Instance);

        await secondRun.ExecuteAsync(CancellationToken.None);

        Assert.Contains(file.FileKey, fileStorageService2.DeletedFileKeys);
        Assert.Empty(submission.PendingFileDeletions);
    }

    [Fact]
    public async Task ExecuteAsync_ProcessesAtMost500SubmissionsPerRun()
    {
        for (var i = 0; i < 501; i++)
        {
            _formSubmissionRepository.Seed(CreateSubmission(Now.AddDays(-1)));
        }

        await CreateJob(Now).ExecuteAsync(CancellationToken.None);

        // 500 is the batch cap - exactly one of the 501 seeded submissions must remain un-anonymized.
        var stillDue = await _formSubmissionRepository.GetDueForAnonymizationAsync(Now, int.MaxValue, CancellationToken.None);
        Assert.Single(stillDue);
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }
}
