namespace GenclikMerkezi.Modules.Candidate.Features.GetCandidateCvContent;

public sealed record CertificateResponse(Guid Id, string Name, string IssuingInstitution, DateOnly? CertificateDate, string? Description);
