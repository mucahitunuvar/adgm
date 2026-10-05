namespace GenclikMerkezi.Modules.Website.Features.GetPublicLegalDocument;

public sealed record PublicLegalDocumentResponse(string Title, string Body, int VersionNumber, DateTime EffectiveAtUtc);
