using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsFeatures;

public sealed class UpdateSiteSettingsFeaturesCommandHandler(
    ISiteSettingsRepository siteSettingsRepository,
    ICurrentUserContext currentUserContext,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSiteSettingsFeaturesCommand, Result>
{
    public async Task<Result> Handle(UpdateSiteSettingsFeaturesCommand request, CancellationToken cancellationToken)
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

        settings.UpdateFeatureFlags(
            request.GlobalSearchEnabled, request.NewsletterEnabled, request.PublicJobListingsEnabled, request.DonationPageEnabled,
            request.AllowSearchEngineIndexing, currentUserContext.UserId!.Value, DateTime.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // ADR-024 §15 (Faz 5 Görev 5): AllowSearchEngineIndexing now feeds the cached sitemap response
        // too (under PublicContentPrefix), not just the public-site bootstrap - InvalidateAllPublic
        // clears both in one call, same broadening Faz 2 Görev 1 already did to this prefix set when
        // the public-site response grew to depend on more sources.
        WebsiteCacheInvalidator.InvalidateAllPublic(cacheService);

        return Result.Success();
    }
}
