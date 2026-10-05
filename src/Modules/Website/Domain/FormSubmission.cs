using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.2 (Faz 3 Görev 4): one public form submission. Everything that determines WHETHER a
// submission is valid (field rules, required consents, content-item linkage) is cross-aggregate
// (needs the live FormDefinition/LegalDocument/ContentItem) and is therefore the Application-layer
// command handler's job, exactly like ContentItem.Create trusts its caller to have already validated
// parentId - this aggregate only enforces what it can check on its own: the handful of fields it is
// structurally impossible to persist without. Status management, archive and anonymization are Görev
// 5's job (ADR-024 §12.2 "Durum yönetimi ... Görev 5'tedir").
public sealed class FormSubmission : AggregateRoot
{
    // ADR-024 §12.2 (Faz 3 Görev 5): the exact edges the master prompt lists - nothing wider. New can
    // only enter review; Approved/Rejected are only reachable from InReview; Completed is only reachable
    // from Approved; Rejected and Completed (the two closing statuses) can only be reopened back to
    // InReview, never anywhere else.
    private static readonly HashSet<(FormSubmissionStatus From, FormSubmissionStatus To)> AllowedTransitions =
    [
        (FormSubmissionStatus.New, FormSubmissionStatus.InReview),
        (FormSubmissionStatus.InReview, FormSubmissionStatus.AwaitingInfo),
        (FormSubmissionStatus.AwaitingInfo, FormSubmissionStatus.InReview),
        (FormSubmissionStatus.InReview, FormSubmissionStatus.Approved),
        (FormSubmissionStatus.InReview, FormSubmissionStatus.Rejected),
        (FormSubmissionStatus.Approved, FormSubmissionStatus.Completed),
        (FormSubmissionStatus.Rejected, FormSubmissionStatus.InReview),
        (FormSubmissionStatus.Completed, FormSubmissionStatus.InReview),
    ];

    private static readonly HashSet<FormSubmissionStatus> ClosingStatuses =
        [FormSubmissionStatus.Rejected, FormSubmissionStatus.Completed];

    private readonly List<FormSubmissionFileAttachment> _fileAttachments = [];
    private readonly List<FormSubmissionAcceptedLegalVersion> _acceptedLegalVersions = [];
    private readonly List<FormSubmissionStatusHistoryEntry> _statusHistory = [];
    private readonly List<FormSubmissionInternalNote> _internalNotes = [];
    private readonly List<FormSubmissionFileDeletion> _pendingFileDeletions = [];

    public Guid FormDefinitionId { get; private set; }

    // §12.2 "gönderim anında alanların o anki tanımı başvuruya kopyalanır": the FormDefinition's
    // DefinitionVersion at the moment this was validated against it - lets a later reader tell
    // whether the live form has since changed shape.
    public int DefinitionVersion { get; private set; }

    // A JSON snapshot of every field's Key/Type/Label(/Options) as of submission time - opaque to
    // this aggregate, the same "Application layer owns the JSON shape" split LayoutBlock.SettingsJson
    // uses, since rendering it (Görev 5's admin detail) needs no domain behavior here.
    public string FieldDefinitionsSnapshotJson { get; private set; } = string.Empty;

    public string ReferenceNumber { get; private set; } = string.Empty;

    public LanguageCode LanguageCode { get; private set; } = null!;

    public DateTime SubmittedAtUtc { get; private set; }

    public Guid? SubmittedByUserId { get; private set; }

    public Guid? SourceContentItemId { get; private set; }

    // Field key -> answer value (string, bool or string[]), JSON-serialized - same opaque-to-domain
    // split as FieldDefinitionsSnapshotJson. File-type answers are not here; they live in
    // FileAttachments instead.
    public string ResponsesJson { get; private set; } = string.Empty;

    public IReadOnlyList<FormSubmissionFileAttachment> FileAttachments => _fileAttachments.AsReadOnly();

    public IReadOnlyList<FormSubmissionAcceptedLegalVersion> AcceptedLegalVersions => _acceptedLegalVersions.AsReadOnly();

    public IReadOnlyList<FormSubmissionStatusHistoryEntry> StatusHistory => _statusHistory.AsReadOnly();

    public IReadOnlyList<FormSubmissionInternalNote> InternalNotes => _internalNotes.AsReadOnly();

    public IReadOnlyList<FormSubmissionFileDeletion> PendingFileDeletions => _pendingFileDeletions.AsReadOnly();

    public FormSubmissionStatus Status { get; private set; }

    public Guid? AssignedToUserId { get; private set; }

    // Set when Status enters a closing status (Rejected/Completed), cleared when reopened back to
    // InReview (ADR-024 §12.2 "ClosedAtUtc: kapanış durumuna geçişte set edilir, yeniden açılınca
    // temizlenir").
    public DateTime? ClosedAtUtc { get; private set; }

    // Internal bookkeeping for the archive job's 30-day countdown only - distinct from ClosedAtUtc
    // because a manual Unarchive restarts the countdown from the unarchive moment, not from the original
    // closing transition (ADR-024 §12.2 "sayaç arşivden çıkarma anından başlar"), while ClosedAtUtc
    // itself must keep reporting the true original closing time.
    public DateTime? ArchiveEligibleSinceUtc { get; private set; }

    public DateTime? ArchivedAtUtc { get; private set; }

    public DateTime? AnonymizedAtUtc { get; private set; }

    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();

    private FormSubmission(
        Guid id, Guid formDefinitionId, int definitionVersion, string fieldDefinitionsSnapshotJson, string referenceNumber,
        LanguageCode languageCode, DateTime submittedAtUtc, Guid? submittedByUserId, Guid? sourceContentItemId, string responsesJson)
        : base(id)
    {
        FormDefinitionId = formDefinitionId;
        DefinitionVersion = definitionVersion;
        FieldDefinitionsSnapshotJson = fieldDefinitionsSnapshotJson;
        ReferenceNumber = referenceNumber;
        LanguageCode = languageCode;
        SubmittedAtUtc = submittedAtUtc;
        SubmittedByUserId = submittedByUserId;
        SourceContentItemId = sourceContentItemId;
        ResponsesJson = responsesJson;
        Status = FormSubmissionStatus.New;
    }

    private FormSubmission()
    {
    }

    // id is supplied by the caller (not generated here) - the handler needs it before this exists, to
    // use as every uploaded file's FileAttachment.OwnerEntityId (files are written before this
    // aggregate is constructed/persisted, ADR-024 §12.2 "dosyalar veritabanı kaydından önce yazılır").
    public static Result<FormSubmission> Create(
        Guid id,
        Guid formDefinitionId,
        int definitionVersion,
        string fieldDefinitionsSnapshotJson,
        string referenceNumber,
        LanguageCode languageCode,
        DateTime submittedAtUtc,
        Guid? submittedByUserId,
        Guid? sourceContentItemId,
        string responsesJson,
        IReadOnlyList<FormSubmissionFileAttachment> fileAttachments,
        IReadOnlyList<FormSubmissionAcceptedLegalVersion> acceptedLegalVersions)
    {
        if (string.IsNullOrWhiteSpace(referenceNumber))
        {
            return Result.Failure<FormSubmission>(
                Error.Validation("FormSubmission.ReferenceNumberRequired", "Reference number is required."));
        }

        var submission = new FormSubmission(
            id, formDefinitionId, definitionVersion, fieldDefinitionsSnapshotJson, referenceNumber, languageCode, submittedAtUtc,
            submittedByUserId, sourceContentItemId, responsesJson);
        submission._fileAttachments.AddRange(fileAttachments);
        submission._acceptedLegalVersions.AddRange(acceptedLegalVersions);

        return Result.Success(submission);
    }

    // ADR-024 §12.2 (Faz 3 Görev 5). "Arşivlenmiş başvuru yalnızca görüntülenebilir" is read as applying
    // to every mutator here, not only status changes - the master prompt's own example - since the
    // alternative (letting notes/assignment through on an archived record) would make "yalnızca
    // görüntülenebilir" misleading. "Anonimleştirilmiş başvurunun durumu değiştirilemez" is explicit
    // for status; AssignTo/AddInternalNote are blocked too for the same reason (nothing legitimate is
    // left to manage once the personal data is gone).
    public Result ChangeStatus(FormSubmissionStatus newStatus, Guid changedByUserId, DateTime changedAtUtc)
    {
        var guardResult = EnsureMutable();
        if (guardResult.IsFailure)
        {
            return guardResult;
        }

        if (!AllowedTransitions.Contains((Status, newStatus)))
        {
            return Result.Failure(Error.Validation(
                "FormSubmission.InvalidStatusTransition", $"Cannot transition from '{Status}' to '{newStatus}'."));
        }

        var previousStatus = Status;
        Status = newStatus;

        if (ClosingStatuses.Contains(newStatus))
        {
            ClosedAtUtc = changedAtUtc;
            ArchiveEligibleSinceUtc = changedAtUtc;
        }
        else if (ClosingStatuses.Contains(previousStatus))
        {
            ClosedAtUtc = null;
            ArchiveEligibleSinceUtc = null;
        }

        _statusHistory.Add(FormSubmissionStatusHistoryEntry.Create(previousStatus, newStatus, changedByUserId, changedAtUtc));
        BumpRowVersion();

        return Result.Success();
    }

    public Result AssignTo(Guid? assignedToUserId)
    {
        var guardResult = EnsureMutable();
        if (guardResult.IsFailure)
        {
            return guardResult;
        }

        AssignedToUserId = assignedToUserId;
        BumpRowVersion();

        return Result.Success();
    }

    public Result AddInternalNote(Guid authorUserId, string? text, DateTime createdAtUtc)
    {
        var guardResult = EnsureMutable();
        if (guardResult.IsFailure)
        {
            return guardResult;
        }

        var noteResult = FormSubmissionInternalNote.Create(authorUserId, text, createdAtUtc);
        if (noteResult.IsFailure)
        {
            return noteResult;
        }

        _internalNotes.Add(noteResult.Value);
        BumpRowVersion();

        return Result.Success();
    }

    // §12.2 "Yönetici elle arşivleyebilir ... (yalnızca kapanış durumundaki başvurular arşivlenebilir)".
    public Result Archive(DateTime archivedAtUtc)
    {
        if (ClosedAtUtc is null)
        {
            return Result.Failure(Error.Conflict(
                "FormSubmission.NotClosed", "Only a submission in a closing status (Rejected/Completed) can be archived."));
        }

        if (ArchivedAtUtc is not null)
        {
            return Result.Failure(Error.Conflict("FormSubmission.AlreadyArchived", "This submission is already archived."));
        }

        ArchivedAtUtc = archivedAtUtc;
        BumpRowVersion();

        return Result.Success();
    }

    // §12.2 "Arşivden çıkarılan ve tekrar kapalı kalan başvuru job tarafından yeniden 30 gün sonra
    // arşivlenir (sayaç arşivden çıkarma anından başlar)" - restarts ArchiveEligibleSinceUtc from now.
    public Result Unarchive(DateTime unarchivedAtUtc)
    {
        if (ArchivedAtUtc is null)
        {
            return Result.Failure(Error.Conflict("FormSubmission.NotArchived", "This submission is not archived."));
        }

        ArchivedAtUtc = null;
        ArchiveEligibleSinceUtc = unarchivedAtUtc;
        BumpRowVersion();

        return Result.Success();
    }

    // §12.2 (Anonimleştirme): clears everything but the reference number, form, language, dates,
    // status and status history (whose user ids "yöneticilere aittir, kalabilir"). File metadata is
    // personal data too (original file names), so every FileAttachment is dropped and its FileKey moved
    // to PendingFileDeletions for the job to delete from storage after this commits. Idempotent: calling
    // this twice (defensive only - the job's own query already excludes already-anonymized submissions)
    // leaves the aggregate unchanged the second time.
    public void Anonymize(DateTime anonymizedAtUtc)
    {
        if (AnonymizedAtUtc is not null)
        {
            return;
        }

        ResponsesJson = "{}";
        SubmittedByUserId = null;

        foreach (var note in _internalNotes)
        {
            note.ClearText();
        }

        foreach (var attachment in _fileAttachments)
        {
            _pendingFileDeletions.Add(FormSubmissionFileDeletion.Create(attachment.File.FileKey));
        }

        _fileAttachments.Clear();

        AnonymizedAtUtc = anonymizedAtUtc;
        BumpRowVersion();
    }

    // Called by AnonymizeExpiredFormSubmissionsJob only after IFileStorageService.DeleteAsync for this
    // key actually succeeded - a failed delete leaves the entry for the next run to retry.
    public void RemovePendingFileDeletion(FormSubmissionFileDeletion deletion) => _pendingFileDeletions.Remove(deletion);

    private Result EnsureMutable()
    {
        if (AnonymizedAtUtc is not null)
        {
            return Result.Failure(Error.Conflict(
                "FormSubmission.Anonymized", "An anonymized submission can no longer be managed."));
        }

        if (ArchivedAtUtc is not null)
        {
            return Result.Failure(Error.Conflict(
                "FormSubmission.Archived", "An archived submission must be unarchived first."));
        }

        return Result.Success();
    }

    private void BumpRowVersion() => RowVersion = Guid.NewGuid().ToByteArray();
}
