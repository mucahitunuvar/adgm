using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.2 (Faz 3 Görev 5): "InternalNotes (child entity: yazan, zaman, metin; maks 2000;
// düzenlenemez, yalnızca eklenir)". The one exception to "never mutates" is ClearText, used only by
// FormSubmission.Anonymize (ADR-024 §12.2 "iç notların metinleri ... temizlenir") - anonymization is a
// retention-compliance operation on the aggregate, not an edit by any admin.
public sealed class FormSubmissionInternalNote : Entity
{
    public const int MaxTextLength = 2000;

    public Guid AuthorUserId { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    private FormSubmissionInternalNote(Guid id, Guid authorUserId, string text, DateTime createdAtUtc)
        : base(id)
    {
        AuthorUserId = authorUserId;
        Text = text;
        CreatedAtUtc = createdAtUtc;
    }

    private FormSubmissionInternalNote()
    {
    }

    public static Result<FormSubmissionInternalNote> Create(Guid authorUserId, string? text, DateTime createdAtUtc)
    {
        var trimmed = (text ?? string.Empty).Trim();
        if (trimmed.Length == 0 || trimmed.Length > MaxTextLength)
        {
            return Result.Failure<FormSubmissionInternalNote>(Error.Validation(
                "FormSubmissionInternalNote.TextInvalid", $"Note text is required and must be at most {MaxTextLength} characters."));
        }

        return Result.Success(new FormSubmissionInternalNote(Guid.NewGuid(), authorUserId, trimmed, createdAtUtc));
    }

    internal void ClearText() => Text = string.Empty;
}
