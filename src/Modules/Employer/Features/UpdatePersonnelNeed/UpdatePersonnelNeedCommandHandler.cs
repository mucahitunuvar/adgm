using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Features.UpdatePersonnelNeed;

public sealed class UpdatePersonnelNeedCommandHandler(
    ICompanyRepository companyRepository,
    IPersonnelNeedRepository personnelNeedRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(EmployerModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePersonnelNeedCommand, Result>
{
    public async Task<Result> Handle(UpdatePersonnelNeedCommand request, CancellationToken cancellationToken)
    {
        var company = await companyRepository.GetByUserIdAsync(currentUserContext.UserId!.Value, cancellationToken);

        if (company is null)
        {
            return Result.Failure(Error.NotFound("Company.NotFound", "No company was found for the current user."));
        }

        if (company.Status != CompanyStatus.Approved)
        {
            return Result.Failure(
                Error.Conflict("PersonnelNeed.CompanyNotApproved", "The company must be approved before updating a personnel need."));
        }

        var personnelNeed = await personnelNeedRepository.GetByIdAsync(request.PersonnelNeedId, cancellationToken);

        if (personnelNeed is null)
        {
            return Result.Failure(Error.NotFound("PersonnelNeed.NotFound", "The specified personnel need could not be found."));
        }

        if (personnelNeed.CompanyId != company.Id)
        {
            return Result.Failure(Error.Forbidden("PersonnelNeed.NotOwner", "You may only update your own company's personnel needs."));
        }

        var updateResult = personnelNeed.Update(
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
            request.DrivingLicensePreferenceIds);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
