using AngleSharp.Html.Dom;
using Ganss.Xss;
using GenclikMerkezi.Modules.Employer.Application.Abstractions;

namespace GenclikMerkezi.Modules.Employer.Infrastructure.Sanitization;

// Görev 2 (Employer public jobs master prompt). Whitelist policy lives in exactly this one place
// (Website.Infrastructure.Sanitization.HtmlSanitizerContentSanitizer's own pattern), built on a
// HtmlSanitizerOptions instance whose collections are cleared before use so nothing from the
// library's own (much broader) defaults leaks in by accident. Deliberately smaller than Website's
// policy - no img/iframe (AboutHtml has no media-embedding feature), no table/heading levels beyond
// what a short "about us" paragraph needs.
public sealed class HtmlSanitizerContentSanitizer : IHtmlContentSanitizer
{
    private readonly HtmlSanitizer _sanitizer;

    public HtmlSanitizerContentSanitizer()
    {
        var options = new HtmlSanitizerOptions();
        options.AllowedTags.Clear();
        options.AllowedAttributes.Clear();
        options.AllowedSchemes.Clear();
        options.AllowedCssProperties.Clear();
        options.UriAttributes.Clear();

        options.AllowedTags.UnionWith(
            ["p", "br", "strong", "b", "em", "i", "u", "ul", "ol", "li", "blockquote", "a", "h2", "h3", "h4"]);
        options.AllowedAttributes.UnionWith(["href", "title", "target"]);
        options.AllowedSchemes.UnionWith(["http", "https", "mailto", "tel"]);
        options.UriAttributes.Add("href");

        _sanitizer = new HtmlSanitizer(options);
        _sanitizer.PostProcessNode += OnPostProcessNode;
    }

    public string Sanitize(string html) => _sanitizer.Sanitize(html ?? string.Empty);

    private static void OnPostProcessNode(object? sender, PostProcessNodeEventArgs e)
    {
        if (e.Node is not IHtmlAnchorElement anchor)
        {
            return;
        }

        // A protocol-relative href ("//evil.com/x") carries no scheme token, so the library's own
        // scheme check never inspects it and lets it through untouched, as if it were an ordinary
        // in-site relative link - a browser instead resolves it against the current page's own
        // scheme, i.e. treats it exactly like an absolute URL to another host.
        var href = anchor.GetAttribute("href");
        if (href is not null && href.StartsWith("//", StringComparison.Ordinal))
        {
            anchor.RemoveAttribute("href");
        }

        if (anchor.Target == "_blank")
        {
            anchor.RelationList.Add("noopener");
            anchor.RelationList.Add("noreferrer");
        }
    }
}
