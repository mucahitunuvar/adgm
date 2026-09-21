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
}
