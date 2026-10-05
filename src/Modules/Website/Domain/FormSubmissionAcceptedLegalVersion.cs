using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.1/§12.2 (Faz 3 Görev 4): one legal document version the submitter accepted at
// submission time - the privacy notice (always present, IsPrivacyNotice = true) and any explicit
// consent the submitter actually accepted ("onaylanan sürümler başvuruya kaydedilir" - a declined
// optional consent is simply absent, never recorded here).
public sealed class FormSubmissionAcceptedLegalVersion : Entity
{
    public LegalDocumentKey LegalDocumentKey { get; private set; } = null!;

    public int VersionNumber { get; private set; }

    public bool IsPrivacyNotice { get; private set; }

    private FormSubmissionAcceptedLegalVersion(Guid id, LegalDocumentKey legalDocumentKey, int versionNumber, bool isPrivacyNotice)
        : base(id)
    {
        LegalDocumentKey = legalDocumentKey;
        VersionNumber = versionNumber;
        IsPrivacyNotice = isPrivacyNotice;
    }

    private FormSubmissionAcceptedLegalVersion()
    {
    }

    public static FormSubmissionAcceptedLegalVersion Create(LegalDocumentKey legalDocumentKey, int versionNumber, bool isPrivacyNotice) =>
        new(Guid.NewGuid(), legalDocumentKey, versionNumber, isPrivacyNotice);
}
