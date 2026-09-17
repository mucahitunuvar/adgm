namespace GenclikMerkezi.Modules.Candidate.Features.GetCandidateCvContent;

public sealed record GetCandidateCvContentResponse(
    Guid Id,
    Guid CandidateCvId,
    string? Summary,
    IReadOnlyList<ExperienceResponse> Experiences,
    IReadOnlyList<EducationResponse> Educations,
    string? ComputerSkills,
    IReadOnlyList<CandidateLanguageResponse> Languages,
    IReadOnlyList<CertificateResponse> Certificates,
    IReadOnlyList<CandidateReferenceResponse> References,
    string? Hobbies,
    string? CvFileUrl);
