using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteLanguage;

public sealed class UpdateSiteLanguageCommandHandler(
    ISiteLanguageRepository siteLanguageRepository,
    ICurrentUserContext currentUserContext,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSiteLanguageCommand, Result>
{
    public async Task<Result> Handle(UpdateSiteLanguageCommand request, CancellationToken cancellationToken)
    {
        var siteLanguage = await siteLanguageRepository.GetByIdAsync(request.Id, cancellationToken);
        if (siteLanguage is null)
        {
            return Result.Failure(Error.NotFound("SiteLanguage.NotFound", "The specified site language could not be found."));
        }

        var renameResult = siteLanguage.Rename(request.Name, request.SortOrder, currentUserContext.UserId!.Value, DateTime.UtcNow);
        if (renameResult.IsFailure)
        {
            return renameResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        WebsiteCacheInvalidator.InvalidatePublicSite(cacheService);

        return Result.Success();
    }
}
