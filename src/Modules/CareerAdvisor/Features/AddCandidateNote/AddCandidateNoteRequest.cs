namespace GenclikMerkezi.Modules.CareerAdvisor.Features.AddCandidateNote;

public sealed record AddCandidateNoteRequest(Guid CandidateUserId, string NoteType, string Content);
