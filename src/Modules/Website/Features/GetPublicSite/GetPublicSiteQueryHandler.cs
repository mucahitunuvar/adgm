using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.LinkTargets;
using GenclikMerkezi.Modules.Website.Application.Media;
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
    IPopupRepository popupRepository,
    IThirdPartyScriptRepository thirdPartyScriptRepository,
    ILegalDocumentRepository legalDocumentRepository,
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

        // ADR-024 §17 (Faz 2 Görev 1 §1.3, extended by Görev 2 §2 and now by Görev 6): shortened to the
        // site-wide nearest future PublishAtUtc/UnpublishAtUtc among ALL content types (a menu link can
        // point at any of them), every slider's slides and every active popup's own schedule.
        var earliestUpcomingContentTransition = await contentItemRepository.GetEarliestUpcomingTransitionAsync(now, cancellationToken);
        var earliestUpcomingSlideTransition = await sliderRepository.GetEarliestUpcomingSlideTransitionAsync(now, cancellationToken);
        var earliestUpcomingPopupTransition = await popupRepository.GetEarliestUpcomingTransitionAsync(now, cancellationToken);
        var ttl = ContentCacheTtlCalculator.Calculate(
            now, [earliestUpcomingContentTransition, earliestUpcomingSlideTransition, earliestUpcomingPopupTransition]);

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
        var popups = await BuildPopupsAsync(resolvedLanguage.Code, defaultLanguage.Code, now, cancellationToken);
        var activeScripts = await thirdPartyScriptRepository.SearchActiveAsync(resolvedLanguage.Code, cancellationToken);
        var scripts = BuildScripts(activeScripts);
        var cookieConsent = await BuildCookieConsentAsync(settings, translation, activeScripts, resolvedLanguage.Code, now, cancellationToken);

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
            menus, popups, cookieConsent, scripts);
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

    // Faz 2 Görev 6 master prompt §6: every currently visible (IsActive + publish window), translated
    // popup, Priority descending - "Hangi pop-up'ın gösterileceğine frontend karar verir". Every
    // popup's own LinkTarget and every Contents-targeting content item id are resolved in one batched
    // LinkTargetResolver call (§1.1 "N+1 yok"), the same single-call-per-section shape BuildMenusAsync
    // already uses for menu links.
    private async Task<IReadOnlyList<PublicPopupResponse>> BuildPopupsAsync(
        LanguageCode languageCode, LanguageCode defaultLanguageCode, DateTime now, CancellationToken cancellationToken)
    {
        var popups = await popupRepository.SearchVisibleAsync(languageCode, now, cancellationToken);
        if (popups.Count == 0)
        {
            return [];
        }

        var allTargets = new List<LinkTarget>();
        foreach (var popup in popups)
        {
            if (!popup.LinkTarget.IsEmpty)
            {
                allTargets.Add(popup.LinkTarget);
            }

            if (popup.Targeting.Kind == PopupTargetingKind.Contents)
            {
                allTargets.AddRange(popup.Targeting.ContentItemIds.Select(id => LinkTarget.ForContent(id).Value));
            }
        }

        var resolutions = await linkTargetResolver.ResolveManyAsync(
            allTargets.Distinct().ToList(), languageCode, defaultLanguageCode, now, cancellationToken);

        var results = new List<PublicPopupResponse>();
        foreach (var popup in popups.OrderByDescending(p => p.Priority))
        {
            // SearchVisibleAsync already filtered to popups with a translation in languageCode.
            var translation = popup.Translations.First(t => t.LanguageCode == languageCode);

            string? href = null;
            string? buttonLabel = translation.ButtonLabel;
            if (!popup.LinkTarget.IsEmpty && resolutions.TryGetValue(popup.LinkTarget, out var resolution) && resolution.IsResolved)
            {
                href = resolution.Href;
            }
            else
            {
                // §6 mirrors Slide's own "link unresolved -> hide button, keep the popup" rule.
                buttonLabel = null;
            }

            var image = popup.ImageMediaId is { } imageMediaId ? await BuildPopupImageAsync(imageMediaId, cancellationToken) : null;
            var targeting = BuildPopupTargetingResponse(popup.Targeting, resolutions);

            results.Add(new PublicPopupResponse(
                popup.Id, popup.DisplayMode.ToString(), translation.Title, translation.Body, buttonLabel, href, image,
                popup.DeviceTarget.ToString(), targeting, popup.DelaySeconds, popup.Frequency.ToString(), popup.FrequencyDays,
                popup.Dismissible, popup.Priority));
        }

        return results;
    }

    private static PublicPopupTargetingResponse BuildPopupTargetingResponse(
        PopupTargeting targeting, IReadOnlyDictionary<LinkTarget, LinkTargetResolution> resolutions)
    {
        if (targeting.Kind != PopupTargetingKind.Contents)
        {
            return new PublicPopupTargetingResponse(targeting.Kind.ToString(), targeting.Paths);
        }

        // §6 "Contents hedeflemesi ... istenen dildeki yollara çevrilir; görünmeyen içerikler listeden
        // çıkarılır".
        var paths = targeting.ContentItemIds
            .Select(id => LinkTarget.ForContent(id).Value)
            .Where(target => resolutions.TryGetValue(target, out var resolution) && resolution.IsResolved)
            .Select(target => resolutions[target].Href!)
            .ToList();

        return new PublicPopupTargetingResponse(targeting.Kind.ToString(), paths);
    }

    private async Task<PublicPopupImageResponse?> BuildPopupImageAsync(Guid imageMediaId, CancellationToken cancellationToken)
    {
        var mediaAsset = await mediaAssetRepository.GetByIdAsync(imageMediaId, cancellationToken);
        if (mediaAsset is null)
        {
            return null;
        }

        var originalUrl = await fileStorageService.GetUrlAsync(mediaAsset.Original.FileKey, cancellationToken);
        string? small = null;
        string? medium = null;
        string? large = null;

        foreach (var variant in mediaAsset.Variants)
        {
            var url = await fileStorageService.GetUrlAsync(variant.File.FileKey, cancellationToken);
            if (variant.VariantName == MediaAssetVariantNames.Small)
            {
                small = url;
            }
            else if (variant.VariantName == MediaAssetVariantNames.Medium)
            {
                medium = url;
            }
            else if (variant.VariantName == MediaAssetVariantNames.Large)
            {
                large = url;
            }
        }

        return new PublicPopupImageResponse(small, medium, large, originalUrl);
    }

    // ADR-024 §13 (Faz 3 Görev 7): every active script's full typed definition - the frontend only
    // ever loads the ones whose Category the visitor has consented to.
    private static IReadOnlyList<PublicSiteScriptResponse> BuildScripts(IReadOnlyList<ThirdPartyScript> activeScripts) =>
        activeScripts
            .Select(s => new PublicSiteScriptResponse(
                s.Id, s.Provider.Kind.ToString(), s.Category.ToString(), s.Placement.ToString(), s.Provider.MeasurementId,
                s.Provider.ContainerId, s.Provider.PixelId, s.Provider.Src, s.Provider.Async, s.Provider.Defer))
            .ToList();

    // ADR-024 §13 (Faz 3 Görev 7): banner text plus, per category, its visitor-facing description and
    // the active scripts that belong to it (name/purpose only - BuildScripts above carries the full
    // typed definitions). PolicyVersion is null when CookiePolicyKey is unset or has no effective
    // version - see PublicSiteCookieConsentResponse's own remarks for what that means to the frontend.
    private async Task<PublicSiteCookieConsentResponse> BuildCookieConsentAsync(
        SiteSettings settings, SiteSettingsTranslation? translation, IReadOnlyList<ThirdPartyScript> activeScripts, LanguageCode languageCode,
        DateTime now, CancellationToken cancellationToken)
    {
        string? policyKey = null;
        int? policyVersion = null;
        if (settings.CookiePolicyKey is not null)
        {
            var document = await legalDocumentRepository.GetByKeyAsync(settings.CookiePolicyKey, cancellationToken);
            var effective = document is null ? null : LegalDocumentEffectiveVersionResolver.Resolve(document.Versions, now);
            if (effective is not null)
            {
                policyKey = settings.CookiePolicyKey.Value;
                policyVersion = effective.VersionNumber;
            }
        }

        var categories = new List<PublicSiteCookieCategoryResponse>
        {
            BuildCookieCategory(
                ThirdPartyScriptCategory.Necessary, translation?.CookieCategoryNecessaryDescription ?? string.Empty, activeScripts, languageCode),
            BuildCookieCategory(
                ThirdPartyScriptCategory.Analytics, translation?.CookieCategoryAnalyticsDescription ?? string.Empty, activeScripts, languageCode),
            BuildCookieCategory(
                ThirdPartyScriptCategory.Marketing, translation?.CookieCategoryMarketingDescription ?? string.Empty, activeScripts, languageCode),
        };

        return new PublicSiteCookieConsentResponse(
            translation?.CookieBannerTitle ?? string.Empty, translation?.CookieBannerText ?? string.Empty, categories, policyKey, policyVersion);
    }

    private static PublicSiteCookieCategoryResponse BuildCookieCategory(
        ThirdPartyScriptCategory category, string description, IReadOnlyList<ThirdPartyScript> scripts, LanguageCode languageCode)
    {
        var categoryScripts = scripts
            .Where(s => s.Category == category)
            .Select(s => s.Translations.FirstOrDefault(t => t.LanguageCode == languageCode))
            .Where(t => t is not null)
            .Select(t => new PublicSiteCookieCategoryScriptResponse(t!.Name, t.Purpose))
            .ToList();

        return new PublicSiteCookieCategoryResponse(category.ToString(), description, categoryScripts);
    }
}
