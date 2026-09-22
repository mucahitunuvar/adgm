namespace GenclikMerkezi.Modules.Interview.Domain;

// Türkçe enum değerleri, PROJECT.md §8.4'ün kendisinin kullandığı isimlendirme - CareerAdvisor.Domain.
// MeetingRequestStatus/Employer.Domain.PersonnelNeedStatus/Matching.Domain.CandidateSuggestionStatus
// ile aynı ikincil konvansiyon.
public enum InterviewStatus
{
    TalepEdildi,
    Planlandi,
    Tamamlandi,
    IptalEdildi,
}
