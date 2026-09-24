using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsMaintenance;

public sealed class UpdateSiteSettingsMaintenanceCommandHandler(
    ISiteSettingsRepository siteSettingsRepository,
    ICurrentUserContext currentUserContext,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSiteSettingsMaintenanceCommand, Result>
{
    public async Task<Result> Handle(UpdateSiteSettingsMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var settings = await siteSettingsRepository.GetAsync(cancellationToken);
        var isNew = settings is null;
        settings ??= SiteSettings.CreateDefault();

        if (!isNew && !request.RowVersion.SequenceEqual(settings.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "SiteSettings.ConcurrencyConflict", "Site settings were changed by someone else. Reload and try again."));
        }

        var translations = new List<(LanguageCode LanguageCode, string? Message)>();
        foreach (var input in request.Translations)
        {
            var languageCodeResult = LanguageCode.Create(input.LanguageCode);
            if (languageCodeResult.IsFailure)
            {
                return languageCodeResult;
            }

            translations.Add((languageCodeResult.Value, input.MaintenanceMessage));
        }

        if (isNew)
        {
            siteSettingsRepository.Add(settings);
        }

        var userId = currentUserContext.UserId!.Value;
        var now = DateTime.UtcNow;

        settings.SetMaintenanceMode(request.MaintenanceModeEnabled, userId, now);

        foreach (var (languageCode, message) in translations)
        {
            settings.SetMaintenanceMessage(languageCode, message, userId, now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        WebsiteCacheInvalidator.InvalidatePublicSite(cacheService);

        return Result.Success();
    }
}
