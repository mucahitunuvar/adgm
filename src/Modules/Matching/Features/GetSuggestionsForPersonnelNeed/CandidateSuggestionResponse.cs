using GenclikMerkezi.Modules.Matching.Domain;

namespace GenclikMerkezi.Modules.Matching.Features.GetSuggestionsForPersonnelNeed;

// GetMySuggestions de bu paylaşılan response'u kullanıyor (JobResponse/GetPublishedJobs deseni).
// Status string olarak döner (CandidateNoteItemResponse.NoteType.ToString() deseni).
public sealed record CandidateSuggestionResponse(
    Guid Id,
    Guid PersonnelNeedId,
    Guid CandidateCvId,
    string? CandidateName,
    Guid SuggestingAdvisorId,
    string Status,
    DateTime CreatedAtUtc,
    Guid? DecidedByAdvisorId,
    DateTime? DecidedAtUtc)
{
    public static CandidateSuggestionResponse FromDomain(CandidateSuggestion suggestion, string? candidateName) => new(
        suggestion.Id,
        suggestion.PersonnelNeedId,
        suggestion.CandidateCvId,
        candidateName,
        suggestion.SuggestingAdvisorId,
        suggestion.Status.ToString(),
        suggestion.CreatedAtUtc,
        suggestion.DecidedByAdvisorId,
        suggestion.DecidedAtUtc);
}
