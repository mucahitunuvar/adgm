using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCertificate;

public sealed record UpdateCertificateCommand(
    Guid CandidateCvId, Guid CertificateId, string Name, string IssuingInstitution, DateOnly? CertificateDate, string? Description)
    : IRequest<Result>;
