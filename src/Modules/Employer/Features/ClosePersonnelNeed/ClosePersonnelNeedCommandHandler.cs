using GenclikMerkezi.Modules.Employer.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Employer.Features.ClosePersonnelNeed;

public sealed class ClosePersonnelNeedCommandHandler(
    IPersonnelNeedRepository personnelNeedRepository,
    [FromKeyedServices(EmployerModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ClosePersonnelNeedCommand, Result>
{
    public async Task<Result> Handle(ClosePersonnelNeedCommand request, CancellationToken cancellationToken)
    {
        var personnelNeed = await personnelNeedRepository.GetByIdAsync(request.PersonnelNeedId, cancellationToken);

        if (personnelNeed is null)
        {
            return Result.Failure(Error.NotFound("PersonnelNeed.NotFound", "The specified personnel need could not be found."));
        }

        var closeResult = personnelNeed.Close(request.ClosedByAdvisorId, request.FulfilledByCandidateCvId, DateTime.UtcNow);

        if (closeResult.IsFailure)
        {
            return closeResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
