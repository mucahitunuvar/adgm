using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Candidate.Features.RemoveCertificate;

public sealed record RemoveCertificateCommand(Guid CandidateCvId, Guid CertificateId) : IRequest<Result>;
