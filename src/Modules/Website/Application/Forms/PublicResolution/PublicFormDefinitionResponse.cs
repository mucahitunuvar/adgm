namespace GenclikMerkezi.Modules.Website.Application.Forms.PublicResolution;

public sealed record PublicFormDefinitionResponse(
    string Key,
    string Title,
    string Description,
    string SubmitButtonLabel,
    IReadOnlyList<PublicFormFieldResponse> Fields,
    PublicFormLegalDocumentReferenceResponse PrivacyNotice,
    IReadOnlyList<PublicFormLegalDocumentReferenceResponse> ExplicitConsents);
