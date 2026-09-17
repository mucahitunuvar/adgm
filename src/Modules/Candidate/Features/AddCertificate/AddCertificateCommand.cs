using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.AddCertificate;

public sealed record AddCertificateCommand(
    Guid CandidateCvId, string Name, string IssuingInstitution, DateOnly? CertificateDate, string? Description)
    : IRequest<Result<Guid>>;
