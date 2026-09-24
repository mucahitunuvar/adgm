using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.DeactivateSiteLanguage;

public sealed class DeactivateSiteLanguageCommandHandler(
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<DeactivateSiteLanguageCommand, Result>
{
    public async Task<Result> Handle(DeactivateSiteLanguageCommand request, CancellationToken cancellationToken)
    {
        var siteLanguage = await siteLanguageRepository.GetByIdAsync(request.Id, cancellationToken);
        if (siteLanguage is null)
        {
            return Result.Failure(Error.NotFound("SiteLanguage.NotFound", "The specified site language could not be found."));
        }

        var deactivateResult = siteLanguage.Deactivate(currentUserContext.UserId!.Value, DateTime.UtcNow);
        if (deactivateResult.IsFailure)
        {
            return deactivateResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        WebsiteCacheInvalidator.InvalidatePublicSite(cacheService);

        return Result.Success();
    }
}
