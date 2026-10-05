using GenclikMerkezi.Modules.Website.Domain;

namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §12.2 (Faz 3 Görev 5): "Liste kişisel veri içermez: referans numarası, form, durum, tarih,
// atanan kişi, ek sayısı" - a projection, not the FormSubmission entity graph, so the list query never
// loads ResponsesJson, SubmittedByUserId or the file/legal-consent collections at all. FormKey stays a
// FormDefinitionKey (not .Value) here because the repository's query selects it directly off the
// FormDefinition row - EF's value converter only runs at materialization, not for a nested member
// access (".Value") inside a server-translated Select, so the string conversion happens here instead,
// after this record is already materialized in memory.
public sealed record FormSubmissionListItem(
    Guid Id,
    string ReferenceNumber,
    FormDefinitionKey FormKey,
    string Status,
    DateTime SubmittedAtUtc,
    Guid? AssignedToUserId,
    int AttachmentCount,
    bool IsArchived,
    byte[] RowVersion);
