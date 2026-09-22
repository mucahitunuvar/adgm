using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Employment.Domain;

// CandidateNote.cs'nin NoteType'sız kopyası (PROJECT.md §12'deki "Takip görüşmeleri" ve "Danışman
// notları" ikisi de bu tek, basit append-only not akışıyla karşılanıyor). EmploymentId, aynı modülün
// kendi aggregate'ine referans - CandidateCvId gibi cross-module bir id değil, bu yüzden isim
// çakışması riski yok, normal şekilde kullanılabilir.
public sealed class EmploymentNote : AggregateRoot
{
    public Guid EmploymentId { get; private set; }

    public Guid CareerAdvisorId { get; private set; }

    public string Content { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private EmploymentNote(Guid id, Guid employmentId, Guid careerAdvisorId, string content, DateTime createdAtUtc)
        : base(id)
    {
        EmploymentId = employmentId;
        CareerAdvisorId = careerAdvisorId;
        Content = content;
        CreatedAtUtc = createdAtUtc;
    }

    public static EmploymentNote Create(Guid employmentId, Guid careerAdvisorId, string content, DateTime createdAtUtc) =>
        new(Guid.NewGuid(), employmentId, careerAdvisorId, content, createdAtUtc);
}
