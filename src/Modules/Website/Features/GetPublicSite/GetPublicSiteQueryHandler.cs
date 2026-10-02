using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.LinkTargets;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Website.Features.GetPublicSite;

// ADR-024 §13/§15: unauthenticated - the frontend calls this once at bootstrap, before any
// page-specific content, to render the site chrome (theme, contact, footer, menus) in the visitor's
// language. Cached per resolved language (ADR-017); invalidated by every SiteSettings-, SiteLanguage-,
// Menu-, ContentItem-, ContentType- and ContentCategory-mutating command (WebsiteCacheInvalidator).
public sealed class GetPublicSiteQueryHandler(
    ISiteSettingsRepository siteSettingsRepository,
    ISiteLanguageRepository siteLanguageRepository,
    IMediaAssetRepository mediaAssetRepository,
    IFileStorageService fileStorageService,
    IMenuRepository menuRepository,
    IContentItemRepository contentItemRepository,
    ISliderRepository sliderRepository,
    LinkTargetResolver linkTargetResolver,
    ICacheService cacheService,
    TimeProvider timeProvider)
    : IRequestHandler<GetPublicSiteQuery, Result<PublicSiteResponse>>
{
    public async Task<Result<PublicSiteResponse>> Handle(GetPublicSiteQuery request, CancellationToken cancellationToken)
    {
        var activeLanguages = await siteLanguageRepository.GetActiveAsync(cancellationToken);
        var defaultLanguage = activeLanguages.First(l => l.IsDefault);

        // Invariant guaranteed by SiteLanguage's own domain rules (Görev 2): the default language
        // can never be deactivated, so there is always exactly one active default to fall back to.
        var resolvedLanguage = (!string.IsNullOrWhiteSpace(request.Lang)
            ? activeLanguages.FirstOrDefault(l => string.Equals(l.Code.Value, request.Lang, StringComparison.OrdinalIgnoreCase))
            : null) ?? defaultLanguage;

        var now = timeProvider.GetUtcNow().UtcDateTime;

        // ADR-024 §17 (Faz 2 Görev 1 §1.3, extended by Görev 2 §2): shortened to the site-wide nearest
        // future PublishAtUtc/UnpublishAtUtc among ALL content types (a menu link can point at any of
        // them) and every slider's slides - designed to be extended further by Görev 6 with pop-up
        // scheduling.
        var earliestUpcomingContentTransition = await contentItemRepository.GetEarliestUpcomingTransitionAsync(now, cancellationToken);
        var earliestUpcomingSlideTransition = await sliderRepository.GetEarliestUpcomingSlideTransitionAsync(now, cancellationToken);
        var ttl = ContentCacheTtlCalculator.Calculate(now, [earliestUpcomingContentTransition, earliestUpcomingSlideTransition]);

        var response = await cacheService.GetOrCreateAsync(
            WebsiteCacheKeys.PublicSite(resolvedLanguage.Code.Value),
            async ct => await BuildResponseAsync(activeLanguages, resolvedLanguage, defaultLanguage, now, ct),
            ttl,
            cancellationToken);

        return Result.Success(response);
    }

    private async Task<PublicSiteResponse> BuildResponseAsync(
        IReadOnlyList<SiteLanguage> activeLanguages, SiteLanguage resolvedLanguage, SiteLanguage defaultLanguage, DateTime now,
        CancellationToken cancellationToken)
    {
        var settings = await siteSettingsRepository.GetAsync(cancellationToken) ?? SiteSettings.CreateDefault();

        var languageResponses = activeLanguages
            .OrderBy(l => l.SortOrder)
            .Select(l => new PublicSiteLanguageResponse(l.Code.Value, l.Name, l.IsDefault, l.SortOrder))
            .ToList();

        var themeResponse = new PublicSiteThemeResponse(settings.Theme.PrimaryColorHex, settings.Theme.SecondaryColorHex, settings.Theme.FontFamily);

        var contactResponse = new PublicContactInfoResponse(
            settings.Contact.Address, settings.Contact.Phone, settings.Contact.Email, settings.Contact.WhatsApp, settings.Contact.MapEmbedUrl);

        var socialLinks = settings.SocialLinks
            .OrderBy(l => l.SortOrder)
            .Select(l => new PublicSocialLinkResponse(l.Platform, l.Url, l.SortOrder))
            .ToList();

        // ADR-024 §13: bank accounts only make sense once the (currently inactive) donation page is
        // enabled - publishing account numbers with no page to explain their purpose would be a
        // pointless disclosure, not a feature.
        var bankAccounts = settings.DonationPageEnabled
            ? settings.BankAccounts
                .Where(a => a.IsActive)
                .OrderBy(a => a.SortOrder)
                .Select(a => new PublicBankAccountResponse(a.Iban.Value, a.BankName, a.AccountHolder, a.Currency.ToString(), a.Description, a.SortOrder))
                .ToList()
            : [];

        var translation = settings.Translations.FirstOrDefault(t => t.LanguageCode == resolvedLanguage.Code);

        var menus = await BuildMenusAsync(resolvedLanguage.Code, defaultLanguage.Code, now, cancellationToken);

        return new PublicSiteResponse(
            languageResponses, resolvedLanguage.Code.Value,
            await ResolveMediaUrlAsync(settings.LogoLightMediaAssetId, cancellationToken),
            await ResolveMediaUrlAsync(settings.LogoDarkMediaAssetId, cancellationToken),
            await ResolveMediaUrlAsync(settings.FaviconMediaAssetId, cancellationToken),
            await ResolveMediaUrlAsync(settings.DefaultOgImageMediaId, cancellationToken),
            themeResponse, contactResponse, socialLinks, bankAccounts,
            translation?.SiteName ?? string.Empty, translation?.Tagline ?? string.Empty,
            translation?.DefaultMetaTitle ?? string.Empty, translation?.DefaultMetaDescription ?? string.Empty,
            translation?.FooterText ?? string.Empty,
            settings.GlobalSearchEnabled, settings.NewsletterEnabled, settings.PublicJobListingsEnabled, settings.DonationPageEnabled,
            settings.MaintenanceModeEnabled, translation?.MaintenanceMessage ?? string.Empty,
            settings.TurnstileSiteKey,
            menus);
    }

    private async Task<string?> ResolveMediaUrlAsync(Guid? mediaAssetId, CancellationToken cancellationToken)
    {
        if (mediaAssetId is null)
        {
            return null;
        }

        var mediaAsset = await mediaAssetRepository.GetByIdAsync(mediaAssetId.Value, cancellationToken);
        return mediaAsset is null ? null : await fileStorageService.GetUrlAsync(mediaAsset.Original.FileKey, cancellationToken);
    }

    private async Task<PublicMenusResponse> BuildMenusAsync(
        LanguageCode languageCode, LanguageCode defaultLanguageCode, DateTime now, CancellationToken cancellationToken)
    {
        var menus = await menuRepository.GetAllAsync(cancellationToken);

        var allTargets = menus.SelectMany(m => m.Items).Select(i => i.LinkTarget).Where(t => !t.IsEmpty).ToList();
        var resolutions = await linkTargetResolver.ResolveManyAsync(allTargets, languageCode, defaultLanguageCode, now, cancellationToken);

        var header = BuildVisibleTree(menus.FirstOrDefault(m => m.Location == MenuLocation.Header), null, languageCode, resolutions);
        var utility = BuildVisibleTree(menus.FirstOrDefault(m => m.Location == MenuLocation.Utility), null, languageCode, resolutions);
        var footer = BuildVisibleTree(menus.FirstOrDefault(m => m.Location == MenuLocation.Footer), null, languageCode, resolutions);

        return new PublicMenusResponse(header, utility, footer);
    }

    // ADR-024 §7 / Faz 2 Görev 1 master prompt §1.3: recursively builds the visible subtree under
    // parentId. An inactive item, one with no translation in languageCode, or one whose link did not
    // resolve is dropped together with its whole subtree (its children are never even visited, since
    // recursion only continues for nodes that passed every check); an empty-of-link group heading with
    // no visible children left is dropped too.
    private static IReadOnlyList<PublicMenuItemResponse> BuildVisibleTree(
        Menu? menu, Guid? parentId, LanguageCode languageCode, IReadOnlyDictionary<LinkTarget, LinkTargetResolution> resolutions)
    {
        if (menu is null)
        {
            return [];
        }

        var results = new List<PublicMenuItemResponse>();

        foreach (var item in menu.Items.Where(i => i.ParentId == parentId && i.IsActive).OrderBy(i => i.SortOrder))
        {
            var translation = item.Translations.FirstOrDefault(t => t.LanguageCode == languageCode);
            if (translation is null)
            {
                continue;
            }

            string? href = null;
            if (!item.LinkTarget.IsEmpty)
            {
                if (!resolutions.TryGetValue(item.LinkTarget, out var resolution) || !resolution.IsResolved)
                {
                    continue;
                }

                href = resolution.Href;
            }

            var children = BuildVisibleTree(menu, item.Id, languageCode, resolutions);
            if (item.LinkTarget.IsEmpty && children.Count == 0)
            {
                continue;
            }

            results.Add(new PublicMenuItemResponse(translation.Label, href, item.OpenInNewTab, item.IconKey, children));
        }

        return results;
    }
}
