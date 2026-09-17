namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCertificate;

public sealed record UpdateCertificateRequest(string Name, string IssuingInstitution, DateOnly? CertificateDate, string? Description);
