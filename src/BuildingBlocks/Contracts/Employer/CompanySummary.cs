namespace GenclikMerkezi.Contracts.Employer;

// ActiveCareerAdvisorSummary/LookupItemSummary ile aynı desen (ADR-016 Decision 2): Contracts
// projesi hiçbir modülün Domain tipine bağımlı olamaz (aksi halde Employer.csproj -> Contracts.csproj
// -> Employer.csproj döngüsü oluşur) - bu yüzden Company.Status (Employer.Domain.CompanyStatus)
// burada taşınmıyor, yalnızca diğer modüllerin gerçekten ihtiyaç duyduğu minimal alanlar var.
// UserId/ContactEmail, Interview modülünün RecordInterviewResult sonrası firmaya bildirim
// göndermesi için eklendi (ApproveJobCommandHandler deseni: notificationModuleContract.
// SendAsync(company.UserId, company.ContactEmail, ...)).
public sealed record CompanySummary(Guid Id, string Name, Guid? CareerAdvisorId, Guid UserId, string ContactEmail);
