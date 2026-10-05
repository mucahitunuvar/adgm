namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §12.2 (Faz 3 Görev 3) "başvurusu olan form silinemez": mirrors LegalDocumentUsage's shape -
// one place a FormDefinition is referenced from. FormSubmission (Görev 4/5) will be a second source
// once it exists; for now ContentItem.FormDefinitionId is the only one.
public sealed record FormDefinitionUsage(string SourceKey, Guid SourceId, string Description, string? Url);
