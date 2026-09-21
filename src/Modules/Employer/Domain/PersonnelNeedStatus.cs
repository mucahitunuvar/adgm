namespace GenclikMerkezi.Modules.Employer.Domain;

// Türkçe enum değerleri, ADR-022 §5 ve ADR-023 §3'ün ikisinin de tutarlı şekilde kullandığı
// isimlendirme - CareerAdvisor.Domain.MeetingRequestStatus/NoteType ile aynı ikincil konvansiyon
// (Company/Job'un İngilizce enum'larından sapma değil).
public enum PersonnelNeedStatus
{
    Taslak,
    KendiHavuzunda,
    GenelHavuzda,
    Karsilandi,
}
