namespace GenclikMerkezi.Modules.Website.Application.Forms.PublicResolution;

// ADR-024 §12.1/§12.2: the effective version of a legal document a form requires consent for - Key lets
// the frontend fetch the full text from /api/v1/public/legal-documents/{key}, VersionNumber is what gets
// echoed back on submission (Görev 4 "acceptedPrivacyNoticeVersion").
public sealed record PublicFormLegalDocumentReferenceResponse(string Key, int VersionNumber, string Title, bool IsRequired);
