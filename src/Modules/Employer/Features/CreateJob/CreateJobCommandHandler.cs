using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Features.CreateJob;

public sealed class CreateJobCommandHandler(
    ICompanyRepository companyRepository,
    IJobRepository jobRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(EmployerModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateJobCommand, Result<CreateJobResponse>>
{
    public async Task<Result<CreateJobResponse>> Handle(CreateJobCommand request, CancellationToken cancellationToken)
    {
        var company = await companyRepository.GetByUserIdAsync(currentUserContext.UserId!.Value, cancellationToken);

        if (company is null)
        {
            return Result.Failure<CreateJobResponse>(
                Error.NotFound("Company.NotFound", "No company was found for the current user."));
        }

        if (company.Status != CompanyStatus.Approved)
        {
            return Result.Failure<CreateJobResponse>(
                Error.Conflict("Job.CompanyNotApproved", "The company must be approved before creating a job."));
        }

        var job = Job.Create(
            company.Id,
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
            request.LanguageRequirements.Select(r => (r.LanguageId, r.LanguageLevelId)).ToList(),
            DateTime.UtcNow);

        jobRepository.Add(job);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateJobResponse(job.Id));
    }
}
