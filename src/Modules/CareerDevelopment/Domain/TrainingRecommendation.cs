using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.CareerDevelopment.Domain;

// Aggregate root (ADR-011). Website.Training ile karıştırılmamalı: bu, "bu adaya şu eğitim önerildi"
// kaydıdır, eğitimin kendisi değil (ADR-011 §"Önemli sınır"). TrainingId, Website modülüne (henüz yok)
// doğrulanmayan bir cross-module referans - Website kurulana kadar var olup olmadığı kontrol edilmez,
// CandidateNote.CandidateCvId ile aynı emsal. DevelopmentPlanId ise aynı modül içi bir referans
// (Guid?, isteğe bağlı) - CreateTrainingRecommendationCommandHandler'da var olup olmadığı doğrulanır.
public sealed class TrainingRecommendation : AggregateRoot
{
    public Guid CandidateCvId { get; private set; }

    public Guid? DevelopmentPlanId { get; private set; }

    public Guid TrainingId { get; private set; }

    public Guid RecommendedByAdvisorId { get; private set; }

    public string? Notes { get; private set; }

    public DateTime RecommendedAtUtc { get; private set; }

    private TrainingRecommendation(
        Guid id, Guid candidateCvId, Guid? developmentPlanId, Guid trainingId, Guid recommendedByAdvisorId,
        string? notes, DateTime recommendedAtUtc)
        : base(id)
    {
        CandidateCvId = candidateCvId;
        DevelopmentPlanId = developmentPlanId;
        TrainingId = trainingId;
        RecommendedByAdvisorId = recommendedByAdvisorId;
        Notes = notes;
        RecommendedAtUtc = recommendedAtUtc;
    }

    public static TrainingRecommendation Create(
        Guid candidateCvId, Guid? developmentPlanId, Guid trainingId, Guid recommendedByAdvisorId,
        string? notes, DateTime recommendedAtUtc) =>
        new(Guid.NewGuid(), candidateCvId, developmentPlanId, trainingId, recommendedByAdvisorId, notes, recommendedAtUtc);
}
