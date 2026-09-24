using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsTheme;

public sealed class UpdateSiteSettingsThemeCommandHandler(
    ISiteSettingsRepository siteSettingsRepository,
    ICurrentUserContext currentUserContext,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSiteSettingsThemeCommand, Result>
{
    public async Task<Result> Handle(UpdateSiteSettingsThemeCommand request, CancellationToken cancellationToken)
    {
        var settings = await siteSettingsRepository.GetAsync(cancellationToken);
        var isNew = settings is null;
        settings ??= SiteSettings.CreateDefault();

        if (!isNew && !request.RowVersion.SequenceEqual(settings.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "SiteSettings.ConcurrencyConflict", "Site settings were changed by someone else. Reload and try again."));
        }

        var themeResult = SiteTheme.Create(request.PrimaryColorHex, request.SecondaryColorHex, request.FontFamily);
        if (themeResult.IsFailure)
        {
            return themeResult;
        }

        if (isNew)
        {
            siteSettingsRepository.Add(settings);
        }

        settings.UpdateTheme(themeResult.Value, currentUserContext.UserId!.Value, DateTime.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        WebsiteCacheInvalidator.InvalidatePublicSite(cacheService);

        return Result.Success();
    }
}
