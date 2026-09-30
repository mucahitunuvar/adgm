namespace GenclikMerkezi.ArchitectureTests;

// ADR-024 §17 (Faz 1b Görev 7): every Website CommandHandler that mutates a ContentItem, ContentType,
// ContentCategory, ContentTag, Video, MediaAsset, Redirect, SiteLanguage or the SiteSettings fields the
// public content/SEO responses read must call WebsiteCacheInvalidator.InvalidatePublicContent after
// commit, or the public list/detail/route-resolution cache goes stale.
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

            var callsInvalidator = File.ReadAllText(sourceFiles[0]).Contains(InvalidationCallExpression, StringComparison.Ordinal);
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
