namespace GenclikMerkezi.Modules.Matching.Domain;

// Türkçe enum değerleri, ADR-022 §5-6'nın kendisinin kullandığı isimlendirme - CareerAdvisor.Domain.
// MeetingRequestStatus/NoteType ve Employer.Domain.PersonnelNeedStatus ile aynı ikincil konvansiyon.
public enum CandidateSuggestionStatus
{
    Onerildi,
    KabulEdildi,
    Reddedildi,
}
