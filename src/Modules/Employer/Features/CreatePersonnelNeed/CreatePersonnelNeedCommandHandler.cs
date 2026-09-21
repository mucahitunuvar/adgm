using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Features.CreatePersonnelNeed;

public sealed class CreatePersonnelNeedCommandHandler(
    ICompanyRepository companyRepository,
    IPersonnelNeedRepository personnelNeedRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(EmployerModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreatePersonnelNeedCommand, Result<CreatePersonnelNeedResponse>>
{
    public async Task<Result<CreatePersonnelNeedResponse>> Handle(
        CreatePersonnelNeedCommand request, CancellationToken cancellationToken)
    {
        var company = await companyRepository.GetByUserIdAsync(currentUserContext.UserId!.Value, cancellationToken);

        if (company is null)
        {
            return Result.Failure<CreatePersonnelNeedResponse>(
                Error.NotFound("Company.NotFound", "No company was found for the current user."));
        }

        if (company.Status != CompanyStatus.Approved)
        {
            return Result.Failure<CreatePersonnelNeedResponse>(
                Error.Conflict("PersonnelNeed.CompanyNotApproved", "The company must be approved before creating a personnel need."));
        }

        var personnelNeed = PersonnelNeed.Create(
            company.Id,
            request.EmploymentTypeId,
            request.WorkLocationTypeId,
            request.PositionId,
            request.DepartmentId,
            request.Quantity,
            request.ProvinceId,
            request.ExperienceLevelId,
            request.DetailsText,
            request.GenderPreferenceIds,
            request.MilitaryStatusPreferenceIds,
            request.EducationLevelPreferenceIds,
            request.DrivingLicensePreferenceIds,
            DateTime.UtcNow);

        personnelNeedRepository.Add(personnelNeed);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreatePersonnelNeedResponse(personnelNeed.Id));
    }
}
