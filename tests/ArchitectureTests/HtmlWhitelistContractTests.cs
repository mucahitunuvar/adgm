using System.Text.Json;
using EmployerWhitelistPolicy = GenclikMerkezi.Modules.Employer.Infrastructure.Sanitization.HtmlWhitelistPolicy;
using WebsiteWhitelistPolicy = GenclikMerkezi.Modules.Website.Infrastructure.Sanitization.HtmlWhitelistPolicy;
using ManifestModules = System.Collections.Generic.SortedDictionary<string, System.Collections.Generic.SortedDictionary<string, string[]>>;

namespace GenclikMerkezi.ArchitectureTests;

// GÖREV A: the Website and Employer modules each own an independent IHtmlContentSanitizer policy
// (no shared kernel type - see AGENTS.md §35, this is module-specific business policy, not a
// universal primitive). Both now build their HtmlSanitizer instance from a dedicated
// HtmlWhitelistPolicy class instead of inline literals, so this test can read the exact same values
// the sanitizer itself uses and publish them as a machine-readable, drift-proof manifest
// (docs/api/html-whitelist.v1.json) - same mechanism as EnumContractTests, applied to a different
// contract. If a tag/attribute/scheme/rule is added, removed or renamed in either policy class
// without updating the manifest, this test fails and names exactly which rule changed.
public class HtmlWhitelistContractTests
{
    [Fact]
    public void Html_Whitelist_Contract_Matches_Committed_Manifest()
    {
        var current = BuildCurrentManifest();
        var manifestPath = Path.Combine(AppContext.BaseDirectory, "TestData", "html-whitelist.v1.json");
        var committedJson = File.ReadAllText(manifestPath);
        var committed = JsonSerializer.Deserialize<ManifestModules>(committedJson)
            ?? throw new InvalidOperationException("docs/api/html-whitelist.v1.json okunamadı veya boş.");

        var differences = Compare(committed, current);

        Assert.True(
            differences.Count == 0,
            "HTML sanitizer whitelist sözleşmesi docs/api/html-whitelist.v1.json ile uyuşmuyor (bir kural " +
            "eklenmiş/silinmiş/değiştirilmiş). Bu kasıtlıysa docs/api/html-whitelist.v1.json'u güncelleyin:" +
            Environment.NewLine + string.Join(Environment.NewLine, differences));
    }

    // İki ayrı yerden çağrılır: bu test (denetim) ve manifest'i ilk kez üreten tek seferlik araç.
    // Aynı mantığı paylaşmaları, "nasıl üretildi" ile "nasıl denetlendi" arasında sapma olmamasını
    // garanti eder (EnumContractTests'teki aynı ilke).
    internal static ManifestModules BuildCurrentManifest()
    {
        var modules = new ManifestModules(StringComparer.Ordinal);

        modules["Employer"] = new SortedDictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["AllowedTags"] = Sorted(EmployerWhitelistPolicy.AllowedTags),
            ["AllowedAttributes"] = Sorted(EmployerWhitelistPolicy.AllowedAttributes),
            ["AllowedHrefSchemes"] = Sorted(EmployerWhitelistPolicy.AllowedHrefSchemes),
            ["RelValuesOnBlankTarget"] = Sorted(EmployerWhitelistPolicy.RelValuesOnBlankTarget),
        };

        modules["Website"] = new SortedDictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["AllowedTags"] = Sorted(WebsiteWhitelistPolicy.AllowedTags),
            ["AllowedAttributes"] = Sorted(WebsiteWhitelistPolicy.AllowedAttributes),
            ["AllowedHrefSchemes"] = Sorted(WebsiteWhitelistPolicy.AllowedHrefSchemes),
            ["RelValuesOnBlankTarget"] = Sorted(WebsiteWhitelistPolicy.RelValuesOnBlankTarget),
            ["UnwrappableTags"] = Sorted(WebsiteWhitelistPolicy.UnwrappableTags),
            ["AllowedIframeHost"] = [WebsiteWhitelistPolicy.AllowedIframeHost],
            ["AllowedIframeEmbedPathPrefix"] = [WebsiteWhitelistPolicy.AllowedIframeEmbedPathPrefix],
            ["ImageSourcePathTraversalTokens"] = Sorted(WebsiteWhitelistPolicy.ImageSourcePathTraversalTokens),
        };

        return modules;
    }

    private static string[] Sorted(IEnumerable<string> values) =>
        values.OrderBy(value => value, StringComparer.Ordinal).ToArray();

    private static List<string> Compare(ManifestModules committed, ManifestModules current)
    {
        var differences = new List<string>();

        foreach (var moduleName in committed.Keys.Union(current.Keys, StringComparer.Ordinal))
        {
            var committedRules = committed.GetValueOrDefault(moduleName) ?? new(StringComparer.Ordinal);
            var currentRules = current.GetValueOrDefault(moduleName) ?? new(StringComparer.Ordinal);

            foreach (var ruleName in committedRules.Keys.Union(currentRules.Keys, StringComparer.Ordinal))
            {
                var hasCommitted = committedRules.TryGetValue(ruleName, out var committedValues);
                var hasCurrent = currentRules.TryGetValue(ruleName, out var currentValues);

                if (!hasCommitted)
                {
                    differences.Add(
                        $"{moduleName}.{ruleName}: manifest'te yok, kodda var (yeni kural): " +
                        $"[{string.Join(", ", currentValues!)}].");
                    continue;
                }

                if (!hasCurrent)
                {
                    differences.Add($"{moduleName}.{ruleName}: manifest'te var, kodda yok (silinmiş kural).");
                    continue;
                }

                var added = currentValues!.Except(committedValues!, StringComparer.Ordinal).ToList();
                var removed = committedValues!.Except(currentValues!, StringComparer.Ordinal).ToList();

                if (added.Count > 0)
                {
                    differences.Add($"{moduleName}.{ruleName}: eklenmiş değer(ler): {string.Join(", ", added)}.");
                }

                if (removed.Count > 0)
                {
                    differences.Add($"{moduleName}.{ruleName}: silinmiş değer(ler): {string.Join(", ", removed)}.");
                }
            }
        }

        return differences;
    }
}
