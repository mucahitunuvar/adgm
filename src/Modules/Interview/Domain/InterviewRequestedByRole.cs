namespace GenclikMerkezi.Modules.Interview.Domain;

// Talebi kim açtı - raporlama için (PROJECT.md §8.4: "Firma veya aday görüşme talebinde bulunabilir").
// Identity.Domain.UserRole'ü kullanamaz (Interview başka bir modülün Domain katmanına bağımlı olamaz,
// AGENTS.md §9/ModuleBoundaryTests) - yalnızca bu akışı başlatabilen iki rol burada yerel olarak var.
public enum InterviewRequestedByRole
{
    Candidate,
    Employer,
}
