namespace GenclikMerkezi.Modules.Interview.Features.RecordInterviewResult;

// Outcome, AddCandidateNoteRequest.NoteType ile aynı sebeple string: System.Text.Json varsayılan
// olarak enum'ları sayı olarak (de)serialize eder (bu projede JsonStringEnumConverter hiç
// yapılandırılmamış), bu yüzden transport'ta string taşınıp Handler'da Enum.Parse ile domain
// enum'a (InterviewResult) çevriliyor.
public sealed record RecordInterviewResultRequest(string Outcome, string? ResultNotes);
