using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeleteThirdPartyScriptTranslation;

public sealed class DeleteThirdPartyScriptTranslationCommandHandler(
    IThirdPartyScriptRepository thirdPartyScriptRepository,
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteThirdPartyScriptTranslationCommand, Result>
{
    public async Task<Result> Handle(DeleteThirdPartyScriptTranslationCommand request, CancellationToken cancellationToken)
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

        var languageCodeResult = LanguageCode.Create(request.LanguageCode);
        if (languageCodeResult.IsFailure)
        {
            return languageCodeResult;
        }

        var defaultLanguage = await siteLanguageRepository.GetDefaultAsync(cancellationToken);
        if (defaultLanguage is null)
        {
            return Result.Failure(Error.Failure("ThirdPartyScript.NoDefaultLanguage", "No default site language is configured."));
        }

        var removeResult = script.RemoveTranslation(
            languageCodeResult.Value, defaultLanguage.Code, currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (removeResult.IsFailure)
        {
            return removeResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidatePublicSite(cacheService);

        return Result.Success();
    }
}
