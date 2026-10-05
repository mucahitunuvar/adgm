using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeactivateFormDefinition;

public sealed class DeactivateFormDefinitionCommandHandler(
    IFormDefinitionRepository formDefinitionRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivateFormDefinitionCommand, Result>
{
    public async Task<Result> Handle(DeactivateFormDefinitionCommand request, CancellationToken cancellationToken)
    {
        var form = await formDefinitionRepository.GetByIdAsync(request.Id, cancellationToken);
        if (form is null)
        {
            return Result.Failure(Error.NotFound("FormDefinition.NotFound", $"Form '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(form.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "FormDefinition.ConcurrencyConflict", "The form was changed by someone else. Reload and try again."));
        }

        var deactivateResult = form.Deactivate(currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (deactivateResult.IsFailure)
        {
            return deactivateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
