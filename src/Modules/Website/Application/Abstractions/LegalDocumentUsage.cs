namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// §12.1 "bir form tarafından kullanılıyorsa silinemez (Görev 3 bu kontrolü bağlar)": mirrors
// SliderUsage's shape - one place a LegalDocument is referenced from (a FormDefinition's
// PrivacyNoticeKey or ExplicitConsentKeys, once Görev 3 adds FormDefinition).
public sealed record LegalDocumentUsage(string SourceKey, Guid SourceId, string Description, string? Url);
