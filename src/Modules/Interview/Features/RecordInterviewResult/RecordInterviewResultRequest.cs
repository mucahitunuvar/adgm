using GenclikMerkezi.Modules.Interview.Domain;

namespace GenclikMerkezi.Modules.Interview.Features.RecordInterviewResult;

public sealed record RecordInterviewResultRequest(InterviewResult Outcome, string? ResultNotes);
