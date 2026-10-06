using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.ThirdPartyScripts;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.Modules.Website.Features.CreateThirdPartyScript;

public sealed class CreateThirdPartyScriptCommandHandler(
    IThirdPartyScriptRepository thirdPartyScriptRepository,
    ISiteLanguageRepository siteLanguageRepository,
    IOptions<WebsiteScriptSettings> scriptSettings,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<CreateThirdPartyScriptCommand, Result<CreateThirdPartyScriptResponse>>
{
    public async Task<Result<CreateThirdPartyScriptResponse>> Handle(
        CreateThirdPartyScriptCommand request, CancellationToken cancellationToken)
    {
        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure<CreateThirdPartyScriptResponse>(
                Error.Failure("ThirdPartyScript.NoDefaultLanguage", "No default site language is configured."));
        }

        var providerResult = ThirdPartyScriptProviderResolver.Resolve(
            request.Provider, request.MeasurementId, request.ContainerId, request.PixelId, request.Src, request.Async, request.Defer,
            scriptSettings.Value.AllowedScriptHosts);
        if (providerResult.IsFailure)
        {
            return Result.Failure<CreateThirdPartyScriptResponse>(providerResult.Error);
        }

        if (!Enum.TryParse<ThirdPartyScriptCategory>(request.Category, out var category))
        {
            return Result.Failure<CreateThirdPartyScriptResponse>(Error.Validation(
                "ThirdPartyScript.CategoryInvalid", "Category must be one of 'Necessary', 'Analytics', 'Marketing'."));
        }

        if (!Enum.TryParse<ThirdPartyScriptPlacement>(request.Placement, out var placement))
        {
            return Result.Failure<CreateThirdPartyScriptResponse>(Error.Validation(
                "ThirdPartyScript.PlacementInvalid", "Placement must be one of 'Head', 'BodyEnd'."));
        }

        var scriptResult = ThirdPartyScript.Create(
            providerResult.Value, category, placement, request.SortOrder, defaultLanguage.Code,
            request.DefaultLanguageName, request.DefaultLanguagePurpose,
            currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (scriptResult.IsFailure)
        {
            return Result.Failure<CreateThirdPartyScriptResponse>(scriptResult.Error);
        }

        thirdPartyScriptRepository.Add(scriptResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidatePublicSite(cacheService);

        return Result.Success(new CreateThirdPartyScriptResponse(scriptResult.Value.Id, defaultLanguage.Code.Value));
    }
}
