using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.UnitTests.Website.Domain;

public class FormSubmissionTests
{
    private static readonly LanguageCode Tr = LanguageCode.Create("tr").Value;
    private static readonly LegalDocumentKey PrivacyNoticeKey = LegalDocumentKey.Create("kvkk-contact").Value;
    private static readonly DateTime Now = new(2026, 10, 5, 12, 0, 0, DateTimeKind.Utc);

    private static FileAttachment CreateFile() =>
        FileAttachment.Create("website-form-attachments/2026/10/05/abc.pdf", "cv.pdf", "application/pdf", 1024, Now, "FormSubmission", Guid.NewGuid());

    [Fact]
    public void Create_WithValidInput_Succeeds()
    {
        var submissionId = Guid.NewGuid();
        var formDefinitionId = Guid.NewGuid();
        var fileAttachment = FormSubmissionFileAttachment.Create("cv", CreateFile());
        var acceptedVersion = FormSubmissionAcceptedLegalVersion.Create(PrivacyNoticeKey, 1, isPrivacyNotice: true);

        var result = FormSubmission.Create(
            submissionId, formDefinitionId, 3, "[]", "GM-2026-000001", Tr, Now, null, null, "{}", [fileAttachment], [acceptedVersion]);

        Assert.True(result.IsSuccess);
        Assert.Equal(submissionId, result.Value.Id);
        Assert.Equal(formDefinitionId, result.Value.FormDefinitionId);
        Assert.Equal(3, result.Value.DefinitionVersion);
        Assert.Equal("GM-2026-000001", result.Value.ReferenceNumber);
        Assert.Equal(FormSubmissionStatus.New, result.Value.Status);
        Assert.Single(result.Value.FileAttachments);
        Assert.Single(result.Value.AcceptedLegalVersions);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutReferenceNumber_Fails(string? referenceNumber)
    {
        var result = FormSubmission.Create(
            Guid.NewGuid(), Guid.NewGuid(), 1, "[]", referenceNumber!, Tr, Now, null, null, "{}", [], []);

        Assert.True(result.IsFailure);
        Assert.Equal("FormSubmission.ReferenceNumberRequired", result.Error.Code);
    }

    [Fact]
    public void Create_WithOptionalFieldsOmitted_Succeeds()
    {
        var result = FormSubmission.Create(
            Guid.NewGuid(), Guid.NewGuid(), 1, "[]", "GM-2026-000002", Tr, Now, submittedByUserId: null, sourceContentItemId: null, "{}", [], []);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.SubmittedByUserId);
        Assert.Null(result.Value.SourceContentItemId);
    }

    private static FormSubmission CreateSubmission(IReadOnlyList<FormSubmissionFileAttachment>? fileAttachments = null)
    {
        var result = FormSubmission.Create(
            Guid.NewGuid(), Guid.NewGuid(), 1, "[]", $"GM-2026-{Random.Shared.Next(100000, 999999)}", Tr, Now, Guid.NewGuid(), null, "{}",
            fileAttachments ?? [], []);
        return result.Value;
    }

    // ADR-024 §12.2 (Faz 3 Görev 5): the exact edges the master prompt lists.
    [Theory]
    [InlineData(FormSubmissionStatus.New, FormSubmissionStatus.InReview)]
    [InlineData(FormSubmissionStatus.InReview, FormSubmissionStatus.AwaitingInfo)]
    [InlineData(FormSubmissionStatus.AwaitingInfo, FormSubmissionStatus.InReview)]
    [InlineData(FormSubmissionStatus.InReview, FormSubmissionStatus.Approved)]
    [InlineData(FormSubmissionStatus.InReview, FormSubmissionStatus.Rejected)]
    [InlineData(FormSubmissionStatus.Approved, FormSubmissionStatus.Completed)]
    [InlineData(FormSubmissionStatus.Rejected, FormSubmissionStatus.InReview)]
    [InlineData(FormSubmissionStatus.Completed, FormSubmissionStatus.InReview)]
    public void ChangeStatus_AllowedTransition_Succeeds(FormSubmissionStatus from, FormSubmissionStatus to)
    {
        var submission = CreateSubmission();
        if (from != FormSubmissionStatus.New)
        {
            DriveTo(submission, from);
        }

        var rowVersionBefore = submission.RowVersion;
        var changedBy = Guid.NewGuid();
        var result = submission.ChangeStatus(to, changedBy, Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(to, submission.Status);
        Assert.NotEqual(rowVersionBefore, submission.RowVersion);
        var lastEntry = submission.StatusHistory[^1];
        Assert.Equal(from, lastEntry.FromStatus);
        Assert.Equal(to, lastEntry.ToStatus);
        Assert.Equal(changedBy, lastEntry.ChangedByUserId);
    }

    [Theory]
    [InlineData(FormSubmissionStatus.New, FormSubmissionStatus.Approved)]
    [InlineData(FormSubmissionStatus.New, FormSubmissionStatus.Rejected)]
    [InlineData(FormSubmissionStatus.AwaitingInfo, FormSubmissionStatus.Approved)]
    [InlineData(FormSubmissionStatus.Approved, FormSubmissionStatus.Rejected)]
    [InlineData(FormSubmissionStatus.Completed, FormSubmissionStatus.Approved)]
    public void ChangeStatus_DisallowedTransition_Fails(FormSubmissionStatus from, FormSubmissionStatus to)
    {
        var submission = CreateSubmission();
        if (from != FormSubmissionStatus.New)
        {
            DriveTo(submission, from);
        }

        var result = submission.ChangeStatus(to, Guid.NewGuid(), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("FormSubmission.InvalidStatusTransition", result.Error.Code);
    }

    [Fact]
    public void ChangeStatus_IntoClosingStatus_SetsClosedAtUtc()
    {
        var submission = CreateSubmission();
        DriveTo(submission, FormSubmissionStatus.InReview);

        submission.ChangeStatus(FormSubmissionStatus.Rejected, Guid.NewGuid(), Now);

        Assert.Equal(Now, submission.ClosedAtUtc);
    }

    [Fact]
    public void ChangeStatus_ReopeningFromClosingStatus_ClearsClosedAtUtc()
    {
        var submission = CreateSubmission();
        DriveTo(submission, FormSubmissionStatus.Rejected);

        submission.ChangeStatus(FormSubmissionStatus.InReview, Guid.NewGuid(), Now);

        Assert.Null(submission.ClosedAtUtc);
    }

    [Fact]
    public void ChangeStatus_WhileArchived_Fails()
    {
        var submission = CreateSubmission();
        DriveTo(submission, FormSubmissionStatus.Completed);
        submission.Archive(Now);

        var result = submission.ChangeStatus(FormSubmissionStatus.InReview, Guid.NewGuid(), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("FormSubmission.Archived", result.Error.Code);
    }

    [Fact]
    public void ChangeStatus_WhileAnonymized_Fails()
    {
        var submission = CreateSubmission();
        submission.Anonymize(Now);

        var result = submission.ChangeStatus(FormSubmissionStatus.InReview, Guid.NewGuid(), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("FormSubmission.Anonymized", result.Error.Code);
    }

    [Fact]
    public void AssignTo_SetsAndClearsAssignee()
    {
        var submission = CreateSubmission();
        var userId = Guid.NewGuid();

        Assert.True(submission.AssignTo(userId).IsSuccess);
        Assert.Equal(userId, submission.AssignedToUserId);

        Assert.True(submission.AssignTo(null).IsSuccess);
        Assert.Null(submission.AssignedToUserId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AddInternalNote_WithEmptyText_Fails(string? text)
    {
        var submission = CreateSubmission();

        var result = submission.AddInternalNote(Guid.NewGuid(), text, Now);

        Assert.True(result.IsFailure);
        Assert.Equal("FormSubmissionInternalNote.TextInvalid", result.Error.Code);
    }

    [Fact]
    public void AddInternalNote_WithTooLongText_Fails()
    {
        var submission = CreateSubmission();

        var result = submission.AddInternalNote(Guid.NewGuid(), new string('a', FormSubmissionInternalNote.MaxTextLength + 1), Now);

        Assert.True(result.IsFailure);
        Assert.Equal("FormSubmissionInternalNote.TextInvalid", result.Error.Code);
    }

    [Fact]
    public void AddInternalNote_WithValidText_Appends()
    {
        var submission = CreateSubmission();
        var authorId = Guid.NewGuid();

        var result = submission.AddInternalNote(authorId, "Takip gerekiyor.", Now);

        Assert.True(result.IsSuccess);
        var note = Assert.Single(submission.InternalNotes);
        Assert.Equal(authorId, note.AuthorUserId);
        Assert.Equal("Takip gerekiyor.", note.Text);
    }

    [Fact]
    public void Archive_WhenNotClosed_Fails()
    {
        var submission = CreateSubmission();

        var result = submission.Archive(Now);

        Assert.True(result.IsFailure);
        Assert.Equal("FormSubmission.NotClosed", result.Error.Code);
    }

    [Fact]
    public void Archive_WhenClosed_Succeeds()
    {
        var submission = CreateSubmission();
        DriveTo(submission, FormSubmissionStatus.Completed);

        var result = submission.Archive(Now);

        Assert.True(result.IsSuccess);
        Assert.Equal(Now, submission.ArchivedAtUtc);
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_Fails()
    {
        var submission = CreateSubmission();
        DriveTo(submission, FormSubmissionStatus.Completed);
        submission.Archive(Now);

        var result = submission.Archive(Now);

        Assert.True(result.IsFailure);
        Assert.Equal("FormSubmission.AlreadyArchived", result.Error.Code);
    }

    [Fact]
    public void Unarchive_WhenNotArchived_Fails()
    {
        var submission = CreateSubmission();

        var result = submission.Unarchive(Now);

        Assert.True(result.IsFailure);
        Assert.Equal("FormSubmission.NotArchived", result.Error.Code);
    }

    [Fact]
    public void Unarchive_RestartsArchiveEligibilityFromUnarchiveMoment()
    {
        var submission = CreateSubmission();
        DriveTo(submission, FormSubmissionStatus.Completed);
        submission.Archive(Now);

        var unarchivedAt = Now.AddDays(5);
        var result = submission.Unarchive(unarchivedAt);

        Assert.True(result.IsSuccess);
        Assert.Null(submission.ArchivedAtUtc);
        Assert.Equal(unarchivedAt, submission.ArchiveEligibleSinceUtc);
        // ClosedAtUtc keeps reporting the true original closing time, unlike the archive countdown.
        Assert.Equal(Now, submission.ClosedAtUtc);
    }

    [Fact]
    public void AssignTo_WhileArchived_Fails()
    {
        var submission = CreateSubmission();
        DriveTo(submission, FormSubmissionStatus.Completed);
        submission.Archive(Now);

        var result = submission.AssignTo(Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Equal("FormSubmission.Archived", result.Error.Code);
    }

    [Fact]
    public void Anonymize_ClearsPersonalDataAndMovesFilesToPendingDeletion()
    {
        var file = CreateFile();
        var fileAttachment = FormSubmissionFileAttachment.Create("cv", file);
        var submission = CreateSubmission([fileAttachment]);
        submission.AddInternalNote(Guid.NewGuid(), "gizli not", Now);

        submission.Anonymize(Now.AddDays(1));

        Assert.Equal(Now.AddDays(1), submission.AnonymizedAtUtc);
        Assert.Equal("{}", submission.ResponsesJson);
        Assert.Null(submission.SubmittedByUserId);
        Assert.Empty(submission.FileAttachments);
        Assert.Equal(string.Empty, Assert.Single(submission.InternalNotes).Text);
        var pending = Assert.Single(submission.PendingFileDeletions);
        Assert.Equal(file.FileKey, pending.FileKey);
    }

    [Fact]
    public void Anonymize_CalledTwice_IsIdempotent()
    {
        var submission = CreateSubmission([FormSubmissionFileAttachment.Create("cv", CreateFile())]);

        submission.Anonymize(Now);
        var pendingAfterFirst = submission.PendingFileDeletions.Count;
        submission.Anonymize(Now.AddDays(1));

        Assert.Equal(Now, submission.AnonymizedAtUtc);
        Assert.Equal(pendingAfterFirst, submission.PendingFileDeletions.Count);
    }

    [Fact]
    public void RemovePendingFileDeletion_RemovesEntry()
    {
        var submission = CreateSubmission([FormSubmissionFileAttachment.Create("cv", CreateFile())]);
        submission.Anonymize(Now);
        var deletion = Assert.Single(submission.PendingFileDeletions);

        submission.RemovePendingFileDeletion(deletion);

        Assert.Empty(submission.PendingFileDeletions);
    }

    // Drives a fresh submission (status New) through the shortest valid path to the target status, so
    // tests can assert behavior "while in status X" without each one re-deriving the path there.
    private static void DriveTo(FormSubmission submission, FormSubmissionStatus target)
    {
        var path = target switch
        {
            FormSubmissionStatus.New => (FormSubmissionStatus[])[],
            FormSubmissionStatus.InReview => [FormSubmissionStatus.InReview],
            FormSubmissionStatus.AwaitingInfo => [FormSubmissionStatus.InReview, FormSubmissionStatus.AwaitingInfo],
            FormSubmissionStatus.Approved => [FormSubmissionStatus.InReview, FormSubmissionStatus.Approved],
            FormSubmissionStatus.Rejected => [FormSubmissionStatus.InReview, FormSubmissionStatus.Rejected],
            FormSubmissionStatus.Completed =>
                [FormSubmissionStatus.InReview, FormSubmissionStatus.Approved, FormSubmissionStatus.Completed],
            _ => throw new ArgumentOutOfRangeException(nameof(target)),
        };

        foreach (var step in path)
        {
            var result = submission.ChangeStatus(step, Guid.NewGuid(), Now);
            if (result.IsFailure)
            {
                throw new InvalidOperationException($"Failed to drive to '{step}': {result.Error.Code}");
            }
        }
    }
}
