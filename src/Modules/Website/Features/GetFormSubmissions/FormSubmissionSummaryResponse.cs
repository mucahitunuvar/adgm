namespace GenclikMerkezi.Modules.Website.Features.GetFormSubmissions;

// ADR-024 §12.2: "Liste kişisel veri içermez" - no answers, no SubmittedByUserId, no file names.
public sealed record FormSubmissionSummaryResponse(
    Guid Id,
    string ReferenceNumber,
    string FormKey,
    string Status,
    DateTime SubmittedAtUtc,
    Guid? AssignedToUserId,
    int AttachmentCount,
    bool IsArchived,
    byte[] RowVersion);
