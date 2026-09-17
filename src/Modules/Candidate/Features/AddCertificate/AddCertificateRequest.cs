namespace GenclikMerkezi.Modules.Candidate.Features.AddCertificate;

public sealed record AddCertificateRequest(string Name, string IssuingInstitution, DateOnly? CertificateDate, string? Description);
