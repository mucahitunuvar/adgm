using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeleteNotFoundPath;

public sealed class DeleteNotFoundPathCommandHandler(
    INotFoundLogRepository notFoundLogRepository, [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteNotFoundPathCommand, Result>
{
    public async Task<Result> Handle(DeleteNotFoundPathCommand request, CancellationToken cancellationToken)
    {
        var notFoundLog = await notFoundLogRepository.GetByIdAsync(request.Id, cancellationToken);
        if (notFoundLog is null)
        {
            return Result.Failure(Error.NotFound("NotFoundLog.NotFound", $"Not-found path log '{request.Id}' could not be found."));
        }

        notFoundLogRepository.Remove(notFoundLog);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
