using GenclikMerkezi.Modules.Employer.Domain;

namespace GenclikMerkezi.UnitTests.Employer.Domain;

public class JobTests
{
    private static Job CreateDraftJob(
        IReadOnlyCollection<Guid>? genderPreferenceIds = null,
        IReadOnlyCollection<(Guid LanguageId, Guid LanguageLevelId)>? languageRequirements = null) =>
        Job.Create(
            Guid.NewGuid(), "Kaynakçı", false, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "<p>Açıklama</p>", Guid.NewGuid(),
            genderPreferenceIds ?? [], [], [], [],
            languageRequirements ?? [], DateTime.UtcNow);

    private static Job TransitionTo(JobStatus status)
    {
        var job = CreateDraftJob();

        if (status == JobStatus.Draft)
        {
            return job;
        }

        job.Submit();

        if (status == JobStatus.UnderReview)
        {
            return job;
        }

        if (status == JobStatus.Published)
        {
            job.Approve(Guid.NewGuid(), DateTime.UtcNow);
            return job;
        }

        if (status == JobStatus.Rejected)
        {
            job.Reject("Eksik bilgi", DateTime.UtcNow);
            return job;
        }

        if (status == JobStatus.SuspendedByAdmin)
        {
            job.Approve(Guid.NewGuid(), DateTime.UtcNow);
            job.Suspend(Guid.NewGuid(), "Uygunsuz içerik", DateTime.UtcNow);
            return job;
        }

        job.RequestRevision("Lütfen açıklamayı detaylandırın", DateTime.UtcNow);
        return job;
    }

    [Fact]
    public void Create_SetsAllFieldsAndDefaultsToDraft()
    {
        var genderId = Guid.NewGuid();
        var languageId = Guid.NewGuid();
        var languageLevelId = Guid.NewGuid();

        var job = CreateDraftJob([genderId], [(languageId, languageLevelId)]);

        Assert.Equal(JobStatus.Draft, job.Status);
        Assert.Equal("Kaynakçı", job.Title);
        Assert.Single(job.GenderPreferences);
        Assert.Equal(genderId, job.GenderPreferences.Single().GenderId);
        Assert.Single(job.LanguageRequirements);
        Assert.Equal(languageId, job.LanguageRequirements.Single().LanguageId);
        Assert.Equal(languageLevelId, job.LanguageRequirements.Single().LanguageLevelId);
        Assert.Null(job.PublishedAtUtc);
        Assert.Null(job.Slug);
    }

    [Theory]
    [InlineData(JobStatus.Draft)]
    [InlineData(JobStatus.RevisionRequested)]
    public void Submit_FromDraftOrRevisionRequested_Succeeds(JobStatus initialStatus)
    {
        var job = TransitionTo(initialStatus);

        var result = job.Submit();

        Assert.True(result.IsSuccess);
        Assert.Equal(JobStatus.UnderReview, job.Status);
    }

    [Theory]
    [InlineData(JobStatus.UnderReview)]
    [InlineData(JobStatus.Published)]
    [InlineData(JobStatus.Rejected)]
    public void Submit_FromOtherStatuses_Fails(JobStatus initialStatus)
    {
        var job = TransitionTo(initialStatus);

        var result = job.Submit();

        Assert.True(result.IsFailure);
        Assert.Equal(initialStatus, job.Status);
    }

    [Fact]
    public void Approve_FromUnderReview_TransitionsDirectlyToPublished()
    {
        var job = TransitionTo(JobStatus.UnderReview);
        var advisorId = Guid.NewGuid();
        var approvedAtUtc = DateTime.UtcNow;

        var result = job.Approve(advisorId, approvedAtUtc);

        Assert.True(result.IsSuccess);
        Assert.Equal(JobStatus.Published, job.Status);
        Assert.Equal(advisorId, job.ReviewedByAdvisorId);
        Assert.Equal(approvedAtUtc, job.ReviewedAtUtc);
        Assert.Equal(approvedAtUtc, job.PublishedAtUtc);
        Assert.Equal(JobSlugGenerator.Generate(job.Title, job.Id), job.Slug);
    }

    // Görev 1 (master prompt): "yeniden yayınlama ... slug'ı korur". Domain'de Approve() yalnızca
    // UnderReview'dan geçer ve Published'tan dönüşün tek yolu Suspend/Reinstate olduğu için (Rejected
    // terminal, RequestRevision yalnızca UnderReview'dan) Approve() bir Job'ın ömründe en fazla bir kez
    // başarıyla çağrılabilir - Approve_FromNonUnderReviewStatus_Fails (Published dahil) bunu zaten
    // doğruluyor. Gerçek "yeniden yayınlama" senaryosu bu yüzden AdminReinstateJob'un Suspend->Reinstate
    // döngüsüdür; Reinstate() Slug'a hiç dokunmaz.
    [Fact]
    public void Reinstate_AfterSuspend_DoesNotChangeSlug()
    {
        var job = TransitionTo(JobStatus.Published);
        var originalSlug = job.Slug;

        job.Suspend(Guid.NewGuid(), "Uygunsuz içerik", DateTime.UtcNow);
        job.Reinstate(DateTime.UtcNow);

        Assert.Equal(JobStatus.Published, job.Status);
        Assert.Equal(originalSlug, job.Slug);
    }

    [Fact]
    public void BackfillSlug_WhenSlugAlreadyAssigned_DoesNotChangeIt()
    {
        var job = TransitionTo(JobStatus.Published);
        var originalSlug = job.Slug;

        job.BackfillSlug();

        Assert.Equal(originalSlug, job.Slug);
    }

    // Approve() artık Slug'ı her zaman atadığı için, "Slug'ı eksik yayınlanmış ilan" durumu normal
    // domain API'siyle üretilemez - bu, yalnızca Slug sütunu eklenmeden önce yayınlanmış geçmiş
    // kayıtları temsil eder (BackfillJobSlugsCommand'ın tek var oluş nedeni). Reflection ile bu
    // geçmiş durum simüle edilir.
    [Fact]
    public void BackfillSlug_WhenSlugIsMissing_AssignsGeneratedSlug()
    {
        var job = TransitionTo(JobStatus.Published);
        typeof(Job).GetProperty(nameof(Job.Slug))!.SetValue(job, null);

        job.BackfillSlug();

        Assert.Equal(JobSlugGenerator.Generate(job.Title, job.Id), job.Slug);
    }

    [Theory]
    [InlineData(JobStatus.Draft)]
    [InlineData(JobStatus.Published)]
    [InlineData(JobStatus.Rejected)]
    [InlineData(JobStatus.RevisionRequested)]
    public void Approve_FromNonUnderReviewStatus_Fails(JobStatus initialStatus)
    {
        var job = TransitionTo(initialStatus);

        var result = job.Approve(Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(initialStatus, job.Status);
    }

    [Fact]
    public void Reject_FromUnderReview_Succeeds()
    {
        var job = TransitionTo(JobStatus.UnderReview);

        var result = job.Reject("Eksik bilgi", DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(JobStatus.Rejected, job.Status);
        Assert.Equal("Eksik bilgi", job.RejectionReason);
    }

    [Theory]
    [InlineData(JobStatus.Draft)]
    [InlineData(JobStatus.Published)]
    [InlineData(JobStatus.Rejected)]
    [InlineData(JobStatus.RevisionRequested)]
    public void Reject_FromNonUnderReviewStatus_Fails(JobStatus initialStatus)
    {
        var job = TransitionTo(initialStatus);

        var result = job.Reject("Eksik bilgi", DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(initialStatus, job.Status);
    }

    [Fact]
    public void RequestRevision_FromUnderReview_Succeeds()
    {
        var job = TransitionTo(JobStatus.UnderReview);

        var result = job.RequestRevision("Lütfen açıklamayı detaylandırın", DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(JobStatus.RevisionRequested, job.Status);
        Assert.Equal("Lütfen açıklamayı detaylandırın", job.RevisionNotes);
    }

    [Theory]
    [InlineData(JobStatus.Draft)]
    [InlineData(JobStatus.Published)]
    [InlineData(JobStatus.Rejected)]
    [InlineData(JobStatus.RevisionRequested)]
    public void RequestRevision_FromNonUnderReviewStatus_Fails(JobStatus initialStatus)
    {
        var job = TransitionTo(initialStatus);

        var result = job.RequestRevision("notlar", DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(initialStatus, job.Status);
    }

    [Theory]
    [InlineData(JobStatus.Draft)]
    [InlineData(JobStatus.RevisionRequested)]
    public void Update_FromDraftOrRevisionRequested_Succeeds(JobStatus initialStatus)
    {
        var job = TransitionTo(initialStatus);
        var newGenderId = Guid.NewGuid();

        var result = job.Update(
            "Yeni Başlık", true, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "<p>Yeni</p>", Guid.NewGuid(), [newGenderId], [], [], [], []);

        Assert.True(result.IsSuccess);
        Assert.Equal("Yeni Başlık", job.Title);
        Assert.True(job.IsForDisabledCandidates);
        Assert.Single(job.GenderPreferences);
        Assert.Equal(newGenderId, job.GenderPreferences.Single().GenderId);
    }

    [Theory]
    [InlineData(JobStatus.UnderReview)]
    [InlineData(JobStatus.Published)]
    [InlineData(JobStatus.Rejected)]
    public void Update_FromOtherStatuses_Fails(JobStatus initialStatus)
    {
        var job = TransitionTo(initialStatus);

        var result = job.Update(
            "Yeni Başlık", true, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            null, Guid.NewGuid(), [], [], [], [], []);

        Assert.True(result.IsFailure);
        Assert.Equal(initialStatus, job.Status);
    }

    [Fact]
    public void SetGenderPreferences_ReplacesExistingPreferences()
    {
        var firstGenderId = Guid.NewGuid();
        var secondGenderId = Guid.NewGuid();
        var job = CreateDraftJob([firstGenderId]);

        job.SetGenderPreferences([secondGenderId]);

        Assert.Single(job.GenderPreferences);
        Assert.Equal(secondGenderId, job.GenderPreferences.Single().GenderId);
    }

    [Fact]
    public void Suspend_FromPublished_Succeeds()
    {
        var job = TransitionTo(JobStatus.Published);
        var suspendedByUserId = Guid.NewGuid();
        var suspendedAtUtc = DateTime.UtcNow;

        var result = job.Suspend(suspendedByUserId, "Uygunsuz içerik", suspendedAtUtc);

        Assert.True(result.IsSuccess);
        Assert.Equal(JobStatus.SuspendedByAdmin, job.Status);
        Assert.Equal(suspendedByUserId, job.SuspendedByUserId);
        Assert.Equal(suspendedAtUtc, job.SuspendedAtUtc);
        Assert.Equal("Uygunsuz içerik", job.SuspensionReason);
    }

    [Theory]
    [InlineData(JobStatus.Draft)]
    [InlineData(JobStatus.UnderReview)]
    [InlineData(JobStatus.Rejected)]
    [InlineData(JobStatus.RevisionRequested)]
    public void Suspend_FromNonPublishedStatus_Fails(JobStatus initialStatus)
    {
        var job = TransitionTo(initialStatus);

        var result = job.Suspend(Guid.NewGuid(), "Uygunsuz içerik", DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(initialStatus, job.Status);
    }

    [Fact]
    public void Reinstate_FromSuspendedByAdmin_TransitionsBackToPublished_WithoutClearingSuspensionFields()
    {
        var job = TransitionTo(JobStatus.SuspendedByAdmin);
        var suspendedByUserId = job.SuspendedByUserId;
        var suspendedAtUtc = job.SuspendedAtUtc;
        var suspensionReason = job.SuspensionReason;

        var result = job.Reinstate(DateTime.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.Equal(JobStatus.Published, job.Status);
        Assert.Equal(suspendedByUserId, job.SuspendedByUserId);
        Assert.Equal(suspendedAtUtc, job.SuspendedAtUtc);
        Assert.Equal(suspensionReason, job.SuspensionReason);
    }

    [Theory]
    [InlineData(JobStatus.Draft)]
    [InlineData(JobStatus.UnderReview)]
    [InlineData(JobStatus.Published)]
    [InlineData(JobStatus.Rejected)]
    [InlineData(JobStatus.RevisionRequested)]
    public void Reinstate_FromNonSuspendedStatus_Fails(JobStatus initialStatus)
    {
        var job = TransitionTo(initialStatus);

        var result = job.Reinstate(DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(initialStatus, job.Status);
    }
}
