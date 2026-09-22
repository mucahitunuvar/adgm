using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Features.AdminSuspendJob;

// ApproveCompanyCommandHandler'daki desenle aynı: doğrudan bir Admin aksiyonu, RejectJob'ın aksine
// firmanın atanmış danışmanının doğrulanmasına gerek yok (rol yetkisi zaten endpoint'te).
public sealed class AdminSuspendJobCommandHandler(
    IJobRepository jobRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(EmployerModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<AdminSuspendJobCommand, Result>
{
    public async Task<Result> Handle(AdminSuspendJobCommand request, CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetByIdAsync(request.JobId, cancellationToken);

        if (job is null)
        {
            return Result.Failure(Error.NotFound("Job.NotFound", "The specified job could not be found."));
        }

        var suspendResult = job.Suspend(currentUserContext.UserId!.Value, request.Reason, DateTime.UtcNow);

        if (suspendResult.IsFailure)
        {
            return suspendResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
