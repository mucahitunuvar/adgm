namespace GenclikMerkezi.Modules.Candidate.Application.Abstractions;

public sealed record CandidateCvPdfDisabilityInfo(
    string CategoryName,
    int Percentage,
    string Description,
    bool HasHealthReport,
    bool UsesMedication,
    bool HasChronicCondition,
    bool HasContagiousDisease,
    bool HasConsciousnessLossRisk);
