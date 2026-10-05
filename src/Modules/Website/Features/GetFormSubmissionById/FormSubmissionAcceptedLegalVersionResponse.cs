namespace GenclikMerkezi.Modules.Website.Features.GetFormSubmissionById;

public sealed record FormSubmissionAcceptedLegalVersionResponse(string LegalDocumentKey, int VersionNumber, bool IsPrivacyNotice);
