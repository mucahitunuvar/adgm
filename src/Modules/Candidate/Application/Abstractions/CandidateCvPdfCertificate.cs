namespace GenclikMerkezi.Modules.Candidate.Application.Abstractions;

public sealed record CandidateCvPdfCertificate(
    string Name, string IssuingInstitution, DateOnly? CertificateDate, string? Description);
