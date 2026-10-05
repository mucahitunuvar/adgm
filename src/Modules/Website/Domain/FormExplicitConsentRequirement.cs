using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §12.1/§12.2 (Faz 3 Görev 3): one of a FormDefinition's (at most 3) explicit-consent checkboxes
// - each references a LegalDocument whose Kind must be ExplicitConsent (checked by the Application-layer
// command handler, which has LegalDocument repository access FormDefinition itself does not). Aydınlatma
// ve açık rıza ayrı onay kutularıdır (ADR-024 §12.1) - this never merges with PrivacyNoticeKey.
public sealed class FormExplicitConsentRequirement : Entity
{
    public LegalDocumentKey LegalDocumentKey { get; private set; } = null!;

    public bool IsRequired { get; private set; }

    private FormExplicitConsentRequirement(Guid id, LegalDocumentKey legalDocumentKey, bool isRequired)
        : base(id)
    {
        LegalDocumentKey = legalDocumentKey;
        IsRequired = isRequired;
    }

    private FormExplicitConsentRequirement()
    {
    }

    public static FormExplicitConsentRequirement Create(LegalDocumentKey legalDocumentKey, bool isRequired) =>
        new(Guid.NewGuid(), legalDocumentKey, isRequired);
}
