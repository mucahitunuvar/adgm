namespace GenclikMerkezi.Modules.Candidate.Application.Abstractions;

public sealed record CandidateCvPdfEducation(
    string EducationLevelName,
    DateOnly StartDate,
    string CompletionStatus,
    DateOnly? EndDate,
    string? DiplomaGradingSystemName,
    decimal? DiplomaGrade,
    string? SchoolName,
    string? ProvinceName,
    string? Description);
