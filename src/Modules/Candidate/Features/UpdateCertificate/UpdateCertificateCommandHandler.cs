using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Candidate.Features.UpdateCertificate;

public sealed class UpdateCertificateCommandHandler(
    ICandidateCvRepository candidateCvRepository,
    ICandidateCvContentRepository candidateCvContentRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(CandidateModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateCertificateCommand, Result>
{
    public async Task<Result> Handle(UpdateCertificateCommand request, CancellationToken cancellationToken)
    {
        var candidateCv = await candidateCvRepository.GetByIdAsync(request.CandidateCvId, cancellationToken);

        if (candidateCv is null)
        {
            return Result.Failure(Error.NotFound("CandidateCv.NotFound", "The specified candidate CV could not be found."));
        }

        if (candidateCv.UserId != currentUserContext.UserId)
        {
            return Result.Failure(Error.Forbidden("CandidateCv.NotOwner", "You may only update your own candidate CV content."));
        }

        var content = await candidateCvContentRepository.GetByCandidateCvIdAsync(candidateCv.Id, cancellationToken);
        var certificate = content?.Certificates.FirstOrDefault(c => c.Id == request.CertificateId);

        if (certificate is null)
        {
            return Result.Failure(Error.NotFound("Certificate.NotFound", "The specified certificate could not be found."));
        }

        certificate.Update(request.Name, request.IssuingInstitution, request.CertificateDate, request.Description);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
