using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Candidate.Domain;

// Nullable owned value object on CandidateCv (Candidate.md's "Engelliyim" checkbox block) -
// CategoryId references ReferenceData's DisabilityCategory lookup, validated at write time by the
// Application layer (ADR-016), not here - Domain must not depend on ReferenceData's Contracts.
public sealed class DisabilityInfo : ValueObject
{
    public Guid CategoryId { get; }

    public int Percentage { get; }

    public string Description { get; }

    public bool HasHealthReport { get; }

    public bool UsesMedication { get; }

    public bool HasChronicCondition { get; }

    public bool HasContagiousDisease { get; }

    public bool HasConsciousnessLossRisk { get; }

    private DisabilityInfo(
        Guid categoryId,
        int percentage,
        string description,
        bool hasHealthReport,
        bool usesMedication,
        bool hasChronicCondition,
        bool hasContagiousDisease,
        bool hasConsciousnessLossRisk)
    {
        CategoryId = categoryId;
        Percentage = percentage;
        Description = description;
        HasHealthReport = hasHealthReport;
        UsesMedication = usesMedication;
        HasChronicCondition = hasChronicCondition;
        HasContagiousDisease = hasContagiousDisease;
        HasConsciousnessLossRisk = hasConsciousnessLossRisk;
    }

    public static DisabilityInfo Create(
        Guid categoryId,
        int percentage,
        string description,
        bool hasHealthReport,
        bool usesMedication,
        bool hasChronicCondition,
        bool hasContagiousDisease,
        bool hasConsciousnessLossRisk) =>
        new(
            categoryId,
            percentage,
            description,
            hasHealthReport,
            usesMedication,
            hasChronicCondition,
            hasContagiousDisease,
            hasConsciousnessLossRisk);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CategoryId;
        yield return Percentage;
        yield return Description;
        yield return HasHealthReport;
        yield return UsesMedication;
        yield return HasChronicCondition;
        yield return HasContagiousDisease;
        yield return HasConsciousnessLossRisk;
    }
}
