namespace GenclikMerkezi.Modules.Employer.Domain;

// Master prompt'un Bağlam bölümü "Draft → Submitted → UnderReview → Published" zincirini
// (DOMAIN.md §10'un genel Job Lifecycle diyagramından) tekrarlıyor, ama Domain metodları bölümündeki
// tek geçiş tanımı Submit()'in doğrudan UnderReview'a geçtiğini söylüyor - ayrı bir Submitted durumuna
// geçen/ondan çıkan hiçbir metod yok. Kullanıcıyla netleştirildi: gerçek iş akışı Draft/RevisionRequested
// -> (Submit) -> UnderReview -> Approve/Reject/RequestRevision, ara bir Submitted durumu yok - bu
// yüzden enum'a alınmadı (asla set edilmeyecek bir değer bırakmamak için).
public enum JobStatus
{
    Draft,
    UnderReview,
    Published,
    Rejected,
    RevisionRequested,
    SuspendedByAdmin,
}
