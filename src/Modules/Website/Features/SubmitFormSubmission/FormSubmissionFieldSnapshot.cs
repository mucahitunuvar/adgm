namespace GenclikMerkezi.Modules.Website.Features.SubmitFormSubmission;

// ADR-024 §12.2 "gönderim anında alanların o anki tanımı başvuruya kopyalanır" - serialized into
// FormSubmission.FieldDefinitionsSnapshotJson so Görev 5's admin detail view can still render the
// submission correctly after the live FormDefinition's fields change shape.
public sealed record FormSubmissionFieldSnapshot(string Key, string Type, string Label, IReadOnlyList<FormSubmissionFieldSnapshotOption>? Options);
