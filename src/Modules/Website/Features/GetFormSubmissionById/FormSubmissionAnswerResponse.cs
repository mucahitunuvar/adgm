namespace GenclikMerkezi.Modules.Website.Features.GetFormSubmissionById;

// ADR-024 §12.2: "yanıtlar alan etiketleriyle eşleştirilmiş olarak (gönderim anındaki tanıma göre)" -
// Label/FieldType come from FormSubmission.FieldDefinitionsSnapshotJson, not the live FormDefinition.
// Value is whatever ResponsesJson held for this field key (string, bool or string[]), or null if the
// field was left unanswered (optional field) or the submission has since been anonymized.
public sealed record FormSubmissionAnswerResponse(string FieldKey, string Label, string FieldType, object? Value);
