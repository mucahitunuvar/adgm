namespace GenclikMerkezi.Modules.Website.Features.GetFormSubmissionById;

public sealed record FormSubmissionDetailResponse(
    Guid Id,
    string ReferenceNumber,
    string FormKey,
    int DefinitionVersion,
    string LanguageCode,
    DateTime SubmittedAtUtc,
    Guid? SubmittedByUserId,
    Guid? SourceContentItemId,
    string Status,
    Guid? AssignedToUserId,
    DateTime? ClosedAtUtc,
    DateTime? ArchivedAtUtc,
    DateTime? AnonymizedAtUtc,
    byte[] RowVersion,
    IReadOnlyList<FormSubmissionAnswerResponse> Answers,
    IReadOnlyList<FormSubmissionFileResponse> Files,
    IReadOnlyList<FormSubmissionAcceptedLegalVersionResponse> AcceptedLegalVersions,
    IReadOnlyList<FormSubmissionStatusHistoryEntryResponse> StatusHistory,
    IReadOnlyList<FormSubmissionInternalNoteResponse> InternalNotes);
