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
    private readonly List<FormSubmissionFileAttachment> _fileAttachments = [];
    private readonly List<FormSubmissionAcceptedLegalVersion> _acceptedLegalVersions = [];

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

    public FormSubmissionStatus Status { get; private set; }

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
}
