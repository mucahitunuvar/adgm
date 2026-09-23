using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.CareerDevelopment.Domain;

// Aggregate root (ADR-011). Danışmanın adaya genel, serbest metin bir tavsiyesi - CandidateNote'un
// NoteType'sız kopyasına benzer basitlikte, ama ayrı bir aggregate (CareerDevelopment'ın kendi
// sorumluluğu, CareerAdvisor modülünün değil).
public sealed class AdvisorRecommendation : AggregateRoot
{
    public Guid CandidateCvId { get; private set; }

    public Guid AdvisorId { get; private set; }

    public string Content { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private AdvisorRecommendation(Guid id, Guid candidateCvId, Guid advisorId, string content, DateTime createdAtUtc)
        : base(id)
    {
        CandidateCvId = candidateCvId;
        AdvisorId = advisorId;
        Content = content;
        CreatedAtUtc = createdAtUtc;
    }

    public static AdvisorRecommendation Create(Guid candidateCvId, Guid advisorId, string content, DateTime createdAtUtc) =>
        new(Guid.NewGuid(), candidateCvId, advisorId, content, createdAtUtc);
}
