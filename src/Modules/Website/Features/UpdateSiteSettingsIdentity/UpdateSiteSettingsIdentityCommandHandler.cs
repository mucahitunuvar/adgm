using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Features.UpdateSiteSettingsIdentity;

public sealed class UpdateSiteSettingsIdentityCommandHandler(
    ISiteSettingsRepository siteSettingsRepository,
    IMediaAssetRepository mediaAssetRepository,
    ICurrentUserContext currentUserContext,
    ICacheService cacheService,
    [FromKeyedServices(WebsiteModuleMarker.UnitOfWorkKey)] IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSiteSettingsIdentityCommand, Result>
{
    public async Task<Result> Handle(UpdateSiteSettingsIdentityCommand request, CancellationToken cancellationToken)
    {
        var settings = await siteSettingsRepository.GetAsync(cancellationToken);
        var isNew = settings is null;
        settings ??= SiteSettings.CreateDefault();

        if (!isNew && !request.RowVersion.SequenceEqual(settings.RowVersion))
        {
            return Result.Failure(Error.Conflict(
                "SiteSettings.ConcurrencyConflict", "Site settings were changed by someone else. Reload and try again."));
        }

        foreach (var mediaAssetId in new[]
                 {
                     request.LogoLightMediaAssetId, request.LogoDarkMediaAssetId, request.FaviconMediaAssetId, request.DefaultOgImageMediaId,
                 })
        {
            if (mediaAssetId is null)
            {
                continue;
            }

            var mediaAsset = await mediaAssetRepository.GetByIdAsync(mediaAssetId.Value, cancellationToken);
            if (mediaAsset is null)
            {
                return Result.Failure(Error.NotFound(
                    "SiteSettings.MediaAssetNotFound", $"Media asset '{mediaAssetId}' could not be found."));
            }
        }

        var translations = new List<(LanguageCode LanguageCode, string? SiteName, string? Tagline, string? MetaTitle, string? MetaDescription, string? FooterText)>();
        foreach (var input in request.Translations)
        {
            var languageCodeResult = LanguageCode.Create(input.LanguageCode);
            if (languageCodeResult.IsFailure)
            {
                return languageCodeResult;
            }

            translations.Add((languageCodeResult.Value, input.SiteName, input.Tagline, input.DefaultMetaTitle, input.DefaultMetaDescription, input.FooterText));
        }

        if (isNew)
        {
            siteSettingsRepository.Add(settings);
        }

        var userId = currentUserContext.UserId!.Value;
        var now = DateTime.UtcNow;

        settings.UpdateIdentity(
            request.LogoLightMediaAssetId, request.LogoDarkMediaAssetId, request.FaviconMediaAssetId, request.DefaultOgImageMediaId, userId, now);

        foreach (var (languageCode, siteName, tagline, metaTitle, metaDescription, footerText) in translations)
        {
            settings.SetIdentityTranslation(languageCode, siteName, tagline, metaTitle, metaDescription, footerText, userId, now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        WebsiteCacheInvalidator.InvalidatePublicSite(cacheService);

        return Result.Success();
    }
}
