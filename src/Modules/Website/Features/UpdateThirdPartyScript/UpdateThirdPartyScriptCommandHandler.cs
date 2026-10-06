using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.ThirdPartyScripts;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.Modules.Website.Features.UpdateThirdPartyScript;

public sealed class UpdateThirdPartyScriptCommandHandler(
    IThirdPartyScriptRepository thirdPartyScriptRepository,
    IOptions<WebsiteScriptSettings> scriptSettings,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateThirdPartyScriptCommand, Result>
{
    public async Task<Result> Handle(UpdateThirdPartyScriptCommand request, CancellationToken cancellationToken)
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

        var providerResult = ThirdPartyScriptProviderResolver.Resolve(
            request.Provider, request.MeasurementId, request.ContainerId, request.PixelId, request.Src, request.Async, request.Defer,
            scriptSettings.Value.AllowedScriptHosts);
        if (providerResult.IsFailure)
        {
            return providerResult;
        }

        if (!Enum.TryParse<ThirdPartyScriptCategory>(request.Category, out var category))
        {
            return Result.Failure(Error.Validation(
                "ThirdPartyScript.CategoryInvalid", "Category must be one of 'Necessary', 'Analytics', 'Marketing'."));
        }

        if (!Enum.TryParse<ThirdPartyScriptPlacement>(request.Placement, out var placement))
        {
            return Result.Failure(Error.Validation("ThirdPartyScript.PlacementInvalid", "Placement must be one of 'Head', 'BodyEnd'."));
        }

        script.Update(
            providerResult.Value, category, placement, request.SortOrder,
            currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidatePublicSite(cacheService);

        return Result.Success();
    }
}
