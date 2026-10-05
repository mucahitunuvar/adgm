using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.2 (Faz 3 Görev 5): "Dosyalar transaction commit'inden sonra depolamadan silinir; silme
// hatası loglanır ve job bir sonraki çalışmada tekrar dener (silinmemiş dosya referansları ayrı
// tutulur)". FormSubmission.Anonymize moves every FileAttachment's FileKey here (and drops the
// attachment's personal-data metadata, e.g. OriginalFileName) in the same transaction that clears the
// rest of the submission; AnonymizeExpiredFormSubmissionsJob then deletes each key from
// IFileStorageService after that commit and removes the entry here only once the physical delete
// actually succeeds - exactly the retry contract FormDefinitionLegalReferenceGuard-adjacent code
// elsewhere in this module never needed, since nothing else defers a storage delete past its own commit.
public sealed class FormSubmissionFileDeletion : Entity
{
    public string FileKey { get; private set; } = string.Empty;

    private FormSubmissionFileDeletion(Guid id, string fileKey)
        : base(id)
    {
        FileKey = fileKey;
    }

    private FormSubmissionFileDeletion()
    {
    }

    public static FormSubmissionFileDeletion Create(string fileKey) => new(Guid.NewGuid(), fileKey);
}
