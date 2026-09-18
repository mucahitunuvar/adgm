namespace GenclikMerkezi.Modules.CareerAdvisor.Features.GetCandidateNotes;

// NoteType string olarak döner (AddCandidateNoteCommand.cs'teki JSON-enum notuyla aynı gerekçe).
public sealed record CandidateNoteItemResponse(Guid Id, Guid CareerAdvisorId, string NoteType, string Content, DateTime CreatedAtUtc);
