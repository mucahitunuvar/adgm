using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsBotProtection;

public sealed class UpdateSiteSettingsBotProtectionCommandHandler(
    ISiteSettingsRepository siteSettingsRepository,
    ICurrentUserContext currentUserContext,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSiteSettingsBotProtectionCommand, Result>
{
    public async Task<Result> Handle(UpdateSiteSettingsBotProtectionCommand request, CancellationToken cancellationToken)
    {
        var settings = await siteSettingsRepository.GetAsync(cancellationToken);
        var isNew = settings is null;
        settings ??= SiteSettings.CreateDefault();

        if (!isNew && !request.RowVersion.SequenceEqual(settings.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "SiteSettings.ConcurrencyConflict", "Site settings were changed by someone else. Reload and try again."));
        }

        if (isNew)
        {
            siteSettingsRepository.Add(settings);
        }

        settings.UpdateBotProtection(request.BotProtectionEnabled, request.TurnstileSiteKey, currentUserContext.UserId!.Value, DateTime.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        WebsiteCacheInvalidator.InvalidatePublicSite(cacheService);

        return Result.Success();
    }
}
