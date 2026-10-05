namespace GenclikMerkezi.Modules.Website.Features.GetFormSubmissionById;

public sealed record FormSubmissionInternalNoteResponse(Guid Id, Guid AuthorUserId, string Text, DateTime CreatedAtUtc);
