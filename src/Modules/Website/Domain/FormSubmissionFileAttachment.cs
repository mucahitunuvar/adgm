using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.2 (Faz 3 Görev 4): a File-type field's uploaded answer - FieldKey ties it back to the
// FormField it answers (resolved against FieldDefinitionsSnapshotJson, not the live FormDefinition,
// since the field may since have been removed/retyped). File carries the already-written
// IFileStorageService metadata (ADR-019) - the handler writes the physical file before this entity is
// even constructed ("dosyalar veritabanı kaydından önce yazılır").
public sealed class FormSubmissionFileAttachment : Entity
{
    public string FieldKey { get; private set; } = string.Empty;

    public FileAttachment File { get; private set; } = null!;

    private FormSubmissionFileAttachment(Guid id, string fieldKey, FileAttachment file)
        : base(id)
    {
        FieldKey = fieldKey;
        File = file;
    }

    private FormSubmissionFileAttachment()
    {
    }

    public static FormSubmissionFileAttachment Create(string fieldKey, FileAttachment file) => new(Guid.NewGuid(), fieldKey, file);
}
