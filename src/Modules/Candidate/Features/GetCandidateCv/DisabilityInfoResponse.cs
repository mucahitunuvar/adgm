namespace GenclikMerkezi.Modules.Candidate.Features.GetCandidateCv;

public sealed record DisabilityInfoResponse(
    Guid CategoryId,
    int Percentage,
    string Description,
    bool HasHealthReport,
    bool UsesMedication,
    bool HasChronicCondition,
    bool HasContagiousDisease,
    bool HasConsciousnessLossRisk);
