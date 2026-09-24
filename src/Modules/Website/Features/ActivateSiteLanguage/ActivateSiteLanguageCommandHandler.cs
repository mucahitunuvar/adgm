using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.ActivateSiteLanguage;

public sealed class ActivateSiteLanguageCommandHandler(
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<ActivateSiteLanguageCommand, Result>
{
    public async Task<Result> Handle(ActivateSiteLanguageCommand request, CancellationToken cancellationToken)
    {
        var siteLanguage = await siteLanguageRepository.GetByIdAsync(request.Id, cancellationToken);
        if (siteLanguage is null)
        {
            return Result.Failure(Error.NotFound("SiteLanguage.NotFound", "The specified site language could not be found."));
        }

        var activateResult = siteLanguage.Activate(currentUserContext.UserId!.Value, DateTime.UtcNow);
        if (activateResult.IsFailure)
        {
            return activateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        WebsiteCacheInvalidator.InvalidatePublicSite(cacheService);

        return Result.Success();
    }
}
