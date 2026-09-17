namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateCvPersonalInfo;

public sealed record DisabilityInfoRequest(
    Guid CategoryId,
    int Percentage,
    string Description,
    bool HasHealthReport,
    bool UsesMedication,
    bool HasChronicCondition,
    bool HasContagiousDisease,
    bool HasConsciousnessLossRisk);
