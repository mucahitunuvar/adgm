using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.CareerAdvisor.Domain;

// CandidateCvId, Candidate modülüne cross-module ID referansı - CareerAdvisor, ADR-022 §1'in tek
// yönlü bağımlılık grafiği (Candidate → CareerAdvisor, tersi değil) gereği Candidate'ın kendi
// verisini sorgulayıp bu id'yi doğrulayamaz (Matching'in PersonnelNeedId/CandidateCvId'yi nasıl
// doğrulamadan tuttuğuyla aynı desen, ADR-022 §5).
public sealed class CandidateNote : AggregateRoot
{
    public Guid CandidateCvId { get; private set; }

    public Guid CareerAdvisorId { get; private set; }

    public NoteType NoteType { get; private set; }

    public string Content { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private CandidateNote(
        Guid id, Guid candidateCvId, Guid careerAdvisorId, NoteType noteType, string content, DateTime createdAtUtc)
        : base(id)
    {
        CandidateCvId = candidateCvId;
        CareerAdvisorId = careerAdvisorId;
        NoteType = noteType;
        Content = content;
        CreatedAtUtc = createdAtUtc;
    }

    public static CandidateNote Create(
        Guid candidateCvId, Guid careerAdvisorId, NoteType noteType, string content, DateTime createdAtUtc) =>
        new(Guid.NewGuid(), candidateCvId, careerAdvisorId, noteType, content, createdAtUtc);
}
