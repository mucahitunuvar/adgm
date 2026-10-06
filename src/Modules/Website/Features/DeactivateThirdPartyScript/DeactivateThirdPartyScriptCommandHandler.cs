using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeactivateThirdPartyScript;

public sealed class DeactivateThirdPartyScriptCommandHandler(
    IThirdPartyScriptRepository thirdPartyScriptRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivateThirdPartyScriptCommand, Result>
{
    public async Task<Result> Handle(DeactivateThirdPartyScriptCommand request, CancellationToken cancellationToken)
    {
        var script = await thirdPartyScriptRepository.GetByIdAsync(request.Id, cancellationToken);
        if (script is null)
        {
            return Result.Failure(Error.NotFound("ThirdPartyScript.NotFound", $"Third-party script '{request.Id}' could not be found."));
        }

        if (!request.RowVersion.SequenceEqual(script.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "ThirdPartyScript.ConcurrencyConflict", "The script was changed by someone else. Reload and try again."));
        }

        script.Deactivate(currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidatePublicSite(cacheService);

        return Result.Success();
    }
}
