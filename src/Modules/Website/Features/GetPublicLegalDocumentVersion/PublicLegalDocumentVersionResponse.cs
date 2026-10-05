namespace GenclikMerkezi.Modules.Website.Features.GetPublicLegalDocumentVersion;

public sealed record PublicLegalDocumentVersionResponse(string Title, string Body, int VersionNumber, DateTime EffectiveAtUtc);
