namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §4.5 (Faz 1b Görev 6): the payload a preview link token carries - which content item, and
// which language (null means "use the default site language", the same convention GetPublicSite/
// ResolveRoute already use).
public sealed record ContentPreviewToken(Guid ContentItemId, string? LanguageCode);
