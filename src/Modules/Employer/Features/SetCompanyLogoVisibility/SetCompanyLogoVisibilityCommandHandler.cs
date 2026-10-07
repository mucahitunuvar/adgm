using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Features.SetCompanyLogoVisibility;

// Görev 2 (Employer public jobs master prompt): GetMyCompanyQueryHandler'ın aynı "mevcut kullanıcının
// kendi firması" çözümü (ICompanyRepository.GetByUserIdAsync) - companyId route/body'den gelmiyor, bu
// yüzden "başka firma değiştiremez" invariant'ı ayrı bir yetki kontrolüne gerek kalmadan sağlanıyor.
public sealed class SetCompanyLogoVisibilityCommandHandler(
    ICompanyRepository companyRepository,
    ICurrentUserContext currentUserContext,
    [FromKeyedServices(EmployerModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<SetCompanyLogoVisibilityCommand, Result>
{
    public async Task<Result> Handle(SetCompanyLogoVisibilityCommand request, CancellationToken cancellationToken)
    {
        var company = await companyRepository.GetByUserIdAsync(currentUserContext.UserId!.Value, cancellationToken);

        if (company is null)
        {
            return Result.Failure(Error.NotFound("Company.NotFound", "No company was found for the current user."));
        }

        if (!request.RowVersion.SequenceEqual(company.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "Company.ConcurrencyConflict", "The company was changed by someone else. Reload and try again."));
        }

        var result = company.SetShowLogoOnWebsite(request.ShowLogoOnWebsite);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
