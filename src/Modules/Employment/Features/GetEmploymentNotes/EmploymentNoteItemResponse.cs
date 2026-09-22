namespace GenclikMerkezi.Modules.Employment.Features.GetEmploymentNotes;

public sealed record EmploymentNoteItemResponse(Guid Id, Guid CareerAdvisorId, string Content, DateTime CreatedAtUtc);
