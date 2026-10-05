using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.2 (Faz 3 Görev 5): "her geçiş StatusHistory'ye eklenir" - one entry per actual
// FormSubmission.ChangeStatus call, never for the initial New status a submission is created with
// (there was no transition into it). Append-only: nothing on this type ever mutates it after creation.
public sealed class FormSubmissionStatusHistoryEntry : Entity
{
    public FormSubmissionStatus FromStatus { get; private set; }

    public FormSubmissionStatus ToStatus { get; private set; }

    public Guid ChangedByUserId { get; private set; }

    public DateTime ChangedAtUtc { get; private set; }

    private FormSubmissionStatusHistoryEntry(
        Guid id, FormSubmissionStatus fromStatus, FormSubmissionStatus toStatus, Guid changedByUserId, DateTime changedAtUtc)
        : base(id)
    {
        FromStatus = fromStatus;
        ToStatus = toStatus;
        ChangedByUserId = changedByUserId;
        ChangedAtUtc = changedAtUtc;
    }

    private FormSubmissionStatusHistoryEntry()
    {
    }

    public static FormSubmissionStatusHistoryEntry Create(
        FormSubmissionStatus fromStatus, FormSubmissionStatus toStatus, Guid changedByUserId, DateTime changedAtUtc) =>
        new(Guid.NewGuid(), fromStatus, toStatus, changedByUserId, changedAtUtc);
}
