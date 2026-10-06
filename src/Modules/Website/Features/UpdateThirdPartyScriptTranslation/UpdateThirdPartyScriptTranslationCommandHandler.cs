using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateThirdPartyScriptTranslation;

public sealed class UpdateThirdPartyScriptTranslationCommandHandler(
    IThirdPartyScriptRepository thirdPartyScriptRepository,
    ICurrentUserContext currentUserContext,
    TimeProvider timeProvider,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateThirdPartyScriptTranslationCommand, Result>
{
    public async Task<Result> Handle(UpdateThirdPartyScriptTranslationCommand request, CancellationToken cancellationToken)
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

        var setResult = script.SetTranslation(
            languageCodeResult.Value, request.Name, request.Purpose,
            currentUserContext.UserId!.Value, timeProvider.GetUtcNow().UtcDateTime);
        if (setResult.IsFailure)
        {
            return setResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        WebsiteCacheInvalidator.InvalidatePublicSite(cacheService);

        return Result.Success();
    }
}
