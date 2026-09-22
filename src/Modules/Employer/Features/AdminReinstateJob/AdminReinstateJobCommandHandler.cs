using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Features.AdminReinstateJob;

public sealed class AdminReinstateJobCommandHandler(
    IJobRepository jobRepository,
    [FromKeyedServices(EmployerModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<AdminReinstateJobCommand, Result>
{
    public async Task<Result> Handle(AdminReinstateJobCommand request, CancellationToken cancellationToken)
    {
        var job = await jobRepository.GetByIdAsync(request.JobId, cancellationToken);

        if (job is null)
        {
            return Result.Failure(Error.NotFound("Job.NotFound", "The specified job could not be found."));
        }

        var reinstateResult = job.Reinstate(DateTime.UtcNow);

        if (reinstateResult.IsFailure)
        {
            return reinstateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
