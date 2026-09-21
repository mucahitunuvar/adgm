using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Features.SubmitJobForReview;

public sealed class SubmitJobForReviewCommandHandler(
    ICompanyRepository companyRepository,
    IJobRepository jobRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(EmployerModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<SubmitJobForReviewCommand, Result>
{
    public async Task<Result> Handle(SubmitJobForReviewCommand request, CancellationToken cancellationToken)
    {
        var company = await companyRepository.GetByUserIdAsync(currentUserContext.UserId!.Value, cancellationToken);

        if (company is null)
        {
            return Result.Failure(Error.NotFound("Company.NotFound", "No company was found for the current user."));
        }

        var job = await jobRepository.GetByIdAsync(request.JobId, cancellationToken);

        if (job is null)
        {
            return Result.Failure(Error.NotFound("Job.NotFound", "The specified job could not be found."));
        }

        if (job.CompanyId != company.Id)
        {
            return Result.Failure(Error.Forbidden("Job.NotOwner", "You may only submit your own company's jobs."));
        }

        var submitResult = job.Submit();

        if (submitResult.IsFailure)
        {
            return submitResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
