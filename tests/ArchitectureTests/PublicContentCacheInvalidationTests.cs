namespace GenclikMerkezi.ArchitectureTests;

// ADR-024 §17 (Faz 1b Görev 7, extended Faz 2 Görev 1): every Website CommandHandler that mutates a
// ContentItem, ContentType, ContentCategory, ContentTag, Video, MediaAsset, Redirect, SiteLanguage or
// the SiteSettings fields the public content/SEO responses read must call
// WebsiteCacheInvalidator.InvalidatePublicContent (directly, or via its InvalidateAllPublic wrapper)
// after commit, or the public list/detail/route-resolution cache goes stale.
//
// NetArchTest's HaveDependencyOn only resolves at type level: several SiteSettings handlers already
// reference the WebsiteCacheInvalidator class for its sibling InvalidatePublicSite call, which would
// make them pass a type-level dependency check without ever calling InvalidatePublicContent. A source
// scan for the exact call expression is used instead - the same "plain reflection over NetArchTest's
// regex-based filter when precision matters more" tradeoff NoGenericRepositoryTests documents, applied
// at the method-call level rather than the type-name level. A fixed exclusion list is still required,
// because a handful of legitimate CommandHandlers genuinely never touch the public content cache: pure
// audit/counter writes that do not change any field a public response could have read, SiteSettings
// sections whose fields never feed a public content/list/detail/SEO response, and a handler that
// persists nothing at all. Each exclusion below is a conscious, individually-justified decision.
public class PublicContentCacheInvalidationTests
{
    private const string InvalidationCallExpression = "WebsiteCacheInvalidator.InvalidatePublicContent(";

    private const string AllPublicInvalidationCallExpression = "WebsiteCacheInvalidator.InvalidateAllPublic(";

    private const string SiteInvalidationCallExpression = "WebsiteCacheInvalidator.InvalidatePublicSite(";

    private static readonly HashSet<string> ExcludedHandlers = new(StringComparer.Ordinal)
    {
        // Pure audit/counter writes - do not change any field a public response could have read.
        "DeleteNotFoundPathCommandHandler",
        "RecordNotFoundPathCommandHandler",
        "RecordRedirectHitCommandHandler",

        // Generates a signed Data Protection token; persists nothing, so there is nothing to invalidate.
        "CreateContentPreviewLinkCommandHandler",

        // SiteSettings sections whose fields never feed a public content list/detail/SEO response,
        // unlike UpdateSiteSettingsIdentityCommandHandler (feeds ContentSeoResolver's site defaults via
        // DefaultOgImageMediaId/DefaultMetaDescription) which IS wired.
        "UpdateSiteSettingsThemeCommandHandler",
        "UpdateSiteSettingsContactCommandHandler",
        "UpdateSiteSettingsFeaturesCommandHandler",
        "UpdateSiteSettingsMaintenanceCommandHandler",
        "UpdateSiteSettingsBotProtectionCommandHandler",
        "UpdateSiteSettingsBankAccountsCommandHandler",
        "UpdateSiteSettingsSubmissionsCommandHandler",
        "UpdateSiteSettingsNewsletterCommandHandler",

        // Faz 2 Görev 1: Menu only feeds the public-site bootstrap response (GetPublicSite) - it never
        // touches the public-content list/detail/route-resolution cache, unlike ContentItem/ContentType/
        // ContentCategory mutations (see the InvalidateAllPublic wrapper those call instead).
        "ReplaceMenuItemsCommandHandler",

        // Faz 3 Görev 4: a submission never changes any publicly cached response - the FormDefinition/
        // ContentItem it was submitted against are unaffected by it, so there is nothing to invalidate.
        "SubmitFormSubmissionCommandHandler",

        // Faz 3 Görev 5: submission management (status/assignment/notes/archive) and the personal data
        // access log are purely admin-side bookkeeping on already-submitted data - none of it is ever
        // read by a public response.
        "ChangeFormSubmissionStatusCommandHandler",
        "AssignFormSubmissionCommandHandler",
        "AddFormSubmissionNoteCommandHandler",
        "ArchiveFormSubmissionCommandHandler",
        "UnarchiveFormSubmissionCommandHandler",
        "RecordPersonalDataAccessCommandHandler",

        // Faz 3 Görev 6: a newsletter subscribe/confirm/unsubscribe or an admin-initiated subscriber
        // deletion never changes any publicly cached response - the newsletter subscribe endpoint reads
        // SiteSettings/LegalDocument directly (never through the public content/site cache), so there is
        // nothing for these to invalidate.
        "SubscribeToNewsletterCommandHandler",
        "ConfirmNewsletterSubscriptionCommandHandler",
        "UnsubscribeFromNewsletterCommandHandler",
        "DeleteNewsletterSubscriberCommandHandler",

        // Faz 3 Görev 7: ThirdPartyScript/cookie-consent settings only ever feed the public-site
        // bootstrap response (GetPublicSite's scripts/cookieConsent fields) - never the public-content
        // list/detail/route-resolution cache - so each of these calls InvalidatePublicSite directly
        // instead of InvalidatePublicContent/InvalidateAllPublic, the same shape every SiteSettings
        // section above that only feeds GetPublicSite already uses.
        "CreateThirdPartyScriptCommandHandler",
        "UpdateThirdPartyScriptCommandHandler",
        "UpdateThirdPartyScriptTranslationCommandHandler",
        "DeleteThirdPartyScriptTranslationCommandHandler",
        "ActivateThirdPartyScriptCommandHandler",
        "DeactivateThirdPartyScriptCommandHandler",
        "DeleteThirdPartyScriptCommandHandler",
        "UpdateSiteSettingsCookieConsentCommandHandler",

        // Faz 3 Görev 7: recording a visitor's cookie-banner decision never changes any publicly
        // cached response - nothing in GetPublicSite's output depends on CookieConsentRecord rows
        // (mirrors SubmitFormSubmissionCommandHandler's own remarks).
        "CreateCookieConsentRecordCommandHandler",

        // Faz 4 Görev 3 (§1 "Kayıt işlemleri çağırmaz" - deliberate exception to the "every mutation
        // invalidates" rule this test otherwise enforces): registration/verification/resend/
        // cancellation never touch anything the public content/list/detail cache reads - capacity and
        // registrationState are never cached (recomputed per request, see EventRegistrationStateInputs'
        // own remarks), so invalidating here would only ever clear unrelated public cache entries for
        // no benefit.
        "CreateEventRegistrationCommandHandler",
        "VerifyEventRegistrationCommandHandler",
        "ResendEventRegistrationVerificationCommandHandler",
        "CancelEventRegistrationCommandHandler",

        // Faz 4 Görev 3: EventPrivacyNoticeKey never feeds a public content/list/detail/SEO response -
        // same bucket as UpdateSiteSettingsCookieConsentCommandHandler above (calls InvalidatePublicSite
        // directly, which this test does not count as InvalidatePublicContent/InvalidateAllPublic).
        "UpdateSiteSettingsEventsCommandHandler",
    };

    [Fact]
    public void MutatingCommandHandlers_Should_InvalidatePublicContentCache_UnlessExplicitlyExcluded()
    {
        var websiteAssembly = ModuleAssemblies.All.Single(a => a.GetName().Name == "GenclikMerkezi.Modules.Website");
        var websiteSourceRoot = Path.Combine(FindRepositoryRoot(), "src", "Modules", "Website");

        var handlerTypeNames = websiteAssembly.SafeGetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("CommandHandler", StringComparison.Ordinal))
            .Select(t => t.Name)
            .ToList();

        Assert.NotEmpty(handlerTypeNames);

        var unexplainedFailures = new List<string>();
        var wronglyExcluded = new List<string>();

        foreach (var handlerTypeName in handlerTypeNames)
        {
            var sourceFiles = Directory.GetFiles(websiteSourceRoot, handlerTypeName + ".cs", SearchOption.AllDirectories);
            Assert.True(sourceFiles.Length == 1, $"Expected exactly one source file for '{handlerTypeName}', found {sourceFiles.Length}.");

            var source = File.ReadAllText(sourceFiles[0]);
            var callsInvalidator = source.Contains(InvalidationCallExpression, StringComparison.Ordinal)
                || source.Contains(AllPublicInvalidationCallExpression, StringComparison.Ordinal);
            var isExcluded = ExcludedHandlers.Contains(handlerTypeName);

            switch (callsInvalidator, isExcluded)
            {
                case (false, false):
                    unexplainedFailures.Add(handlerTypeName);
                    break;
                case (true, true):
                    wronglyExcluded.Add(handlerTypeName);
                    break;
            }
        }

        Assert.True(
            unexplainedFailures.Count == 0,
            "The following CommandHandlers mutate Website state but never call WebsiteCacheInvalidator.InvalidatePublicContent, "
                + "and are not in the documented exclusion list: " + string.Join(", ", unexplainedFailures));

        Assert.True(
            wronglyExcluded.Count == 0,
            "These handlers are listed as excluded but now DO call WebsiteCacheInvalidator.InvalidatePublicContent - "
                + "remove them from the exclusion list: " + string.Join(", ", wronglyExcluded));

        // Keeps the exclusion list itself honest against renamed/removed handlers.
        var staleExclusions = ExcludedHandlers.Except(handlerTypeNames).ToList();

        Assert.True(
            staleExclusions.Count == 0,
            "These excluded handler names no longer exist in the Website module - remove them from the exclusion list: "
                + string.Join(", ", staleExclusions));
    }

    // ADR-024 §17 (Faz 2 Görev 1): the public site bootstrap response now embeds menus (Menu ->
    // MenuItem -> LinkTarget resolves against ContentItem/ContentType), so every ContentItem,
    // ContentType and ContentCategory mutation must ALSO clear the public-site prefix - not just
    // public-content's - or a menu link to newly-hidden/renamed content stays stale. No exclusion list:
    // every handler for these three aggregates changes something a menu link could depend on (directly,
    // or coarse-grained per ADR-024's own "kaba taneli temizlik kabul edilebilir").
    [Fact]
    public void ContentItemContentTypeAndContentCategoryCommandHandlers_Should_AlsoInvalidatePublicSiteCache()
    {
        var websiteAssembly = ModuleAssemblies.All.Single(a => a.GetName().Name == "GenclikMerkezi.Modules.Website");
        var websiteSourceRoot = Path.Combine(FindRepositoryRoot(), "src", "Modules", "Website");

        var handlerTypeNames = websiteAssembly.SafeGetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("CommandHandler", StringComparison.Ordinal))
            .Select(t => t.Name)
            .Where(name => name.Contains("ContentItem", StringComparison.Ordinal)
                || name.Contains("ContentType", StringComparison.Ordinal)
                || name.Contains("ContentCategory", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(handlerTypeNames);

        var unexplainedFailures = new List<string>();

        foreach (var handlerTypeName in handlerTypeNames)
        {
            var sourceFiles = Directory.GetFiles(websiteSourceRoot, handlerTypeName + ".cs", SearchOption.AllDirectories);
            Assert.True(sourceFiles.Length == 1, $"Expected exactly one source file for '{handlerTypeName}', found {sourceFiles.Length}.");

            var source = File.ReadAllText(sourceFiles[0]);
            var callsSiteInvalidator = source.Contains(SiteInvalidationCallExpression, StringComparison.Ordinal)
                || source.Contains(AllPublicInvalidationCallExpression, StringComparison.Ordinal);

            if (!callsSiteInvalidator)
            {
                unexplainedFailures.Add(handlerTypeName);
            }
        }

        Assert.True(
            unexplainedFailures.Count == 0,
            "The following ContentItem/ContentType/ContentCategory CommandHandlers never call "
                + "WebsiteCacheInvalidator.InvalidatePublicSite (directly or via InvalidateAllPublic): "
                + string.Join(", ", unexplainedFailures));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory!.FullName;
    }
}
