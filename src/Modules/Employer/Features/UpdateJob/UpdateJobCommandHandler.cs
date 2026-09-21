using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Features.UpdateJob;

public sealed class UpdateJobCommandHandler(
    ICompanyRepository companyRepository,
    IJobRepository jobRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(EmployerModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateJobCommand, Result>
{
    public async Task<Result> Handle(UpdateJobCommand request, CancellationToken cancellationToken)
    {
        var company = await companyRepository.GetByUserIdAsync(currentUserContext.UserId!.Value, cancellationToken);

        if (company is null)
        {
            return Result.Failure(Error.NotFound("Company.NotFound", "No company was found for the current user."));
        }

        if (company.Status != CompanyStatus.Approved)
        {
            return Result.Failure(
                Error.Conflict("Job.CompanyNotApproved", "The company must be approved before updating a job."));
        }

        var job = await jobRepository.GetByIdAsync(request.JobId, cancellationToken);

        if (job is null)
        {
            return Result.Failure(Error.NotFound("Job.NotFound", "The specified job could not be found."));
        }

        if (job.CompanyId != company.Id)
        {
            return Result.Failure(Error.Forbidden("Job.NotOwner", "You may only update your own company's jobs."));
        }

        var updateResult = job.Update(
            request.Title,
            request.IsForDisabledCandidates,
            request.EmploymentTypeId,
            request.WorkLocationTypeId,
            request.PositionId,
            request.DepartmentId,
            request.ProvinceId,
            request.DescriptionHtml,
            request.ExperienceLevelId,
            request.GenderPreferenceIds,
            request.MilitaryStatusPreferenceIds,
            request.EducationLevelPreferenceIds,
            request.DrivingLicensePreferenceIds,
            request.LanguageRequirements.Select(r => (r.LanguageId, r.LanguageLevelId)).ToList());

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
