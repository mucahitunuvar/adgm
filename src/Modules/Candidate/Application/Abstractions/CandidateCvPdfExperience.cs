namespace GenclikMerkezi.Modules.Candidate.Application.Abstractions;

public sealed record CandidateCvPdfExperience(
    string CompanyName,
    string? PositionName,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsCurrentJob,
    string? SectorName,
    string? WorkFieldName,
    string? EmploymentTypeName,
    string? CountryName,
    string? ProvinceName,
    string? JobDescription);
