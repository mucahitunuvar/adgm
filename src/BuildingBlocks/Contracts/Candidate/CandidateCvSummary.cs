namespace GenclikMerkezi.Contracts.Candidate;

// ActiveCareerAdvisorSummary/CompanySummary ile aynı desen (ADR-016 Decision 2): Contracts projesi
// hiçbir modülün Domain tipine bağımlı olamaz, bu yüzden CandidateCv'nin diğer tüm alanları
// taşınmaz - yalnızca diğer modüllerin (Matching, Görev 7) gerçekten ihtiyaç duyduğu minimal alanlar.
// UserId/Email, Interview modülünün RecordInterviewResult sonrası adaya bildirim göndermesi için
// eklendi (CompanySummary.UserId/ContactEmail ile aynı gerekçe).
public sealed record CandidateCvSummary(Guid Id, string FirstName, string LastName, Guid? CareerAdvisorId, Guid UserId, string Email);
