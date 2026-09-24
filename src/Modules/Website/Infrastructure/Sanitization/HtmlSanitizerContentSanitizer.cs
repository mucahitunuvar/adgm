using AngleSharp.Html.Dom;
using Ganss.Xss;
using GenclikMerkezi.BuildingBlocks.Infrastructure.FileStorage;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Sanitization;

// ADR-024 §16 (Görev 3): the whitelist policy lives in exactly this one place, built on a
// HtmlSanitizerOptions instance whose collections are cleared before use, so nothing from the
// library's own (much broader) defaults leaks in by accident - every allowed tag/attribute/scheme
// below is a deliberate choice, not an inherited one.
//
// img/src and iframe/src need stricter rules than HtmlSanitizer's built-in scheme whitelist can
// express on its own (a's href and img's src would otherwise have to share one global AllowedSchemes
// set, and only href is meant to allow absolute http/https/mailto/tel), so both are validated
// directly against the raw attribute string in PostProcessNode instead of via AllowedSchemes.
public sealed class HtmlSanitizerContentSanitizer : IHtmlContentSanitizer
{
    private const string YouTubeNoCookieHost = "www.youtube-nocookie.com";
    private const string YouTubeNoCookieEmbedPathPrefix = "/embed/";

    private readonly HtmlSanitizer _sanitizer;
    private readonly string _publicMediaRootPrefix;

    public HtmlSanitizerContentSanitizer(IOptions<FileStorageSettings> fileStorageOptions)
    {
        _publicMediaRootPrefix = fileStorageOptions.Value.PublicRequestPath.TrimEnd('/') + "/";

        var options = new HtmlSanitizerOptions();
        options.AllowedTags.Clear();
        options.AllowedAttributes.Clear();
        options.AllowedSchemes.Clear();
        options.AllowedCssProperties.Clear();
        options.UriAttributes.Clear();

        options.AllowedTags.UnionWith(
        [
            "p", "br", "strong", "b", "em", "i", "u", "s", "h2", "h3", "h4", "ul", "ol", "li", "blockquote",
            "a", "img", "figure", "figcaption", "table", "thead", "tbody", "tr", "th", "td", "hr", "iframe",
        ]);
        options.AllowedAttributes.UnionWith(["href", "title", "target", "src", "alt", "width", "height"]);

        // Only href goes through the library's own scheme check - relative hrefs (in-site links)
        // pass through unrestricted, absolute ones are limited to these four schemes. img/iframe src
        // are deliberately NOT registered here (see class remarks) and validated below instead.
        options.AllowedSchemes.UnionWith(["http", "https", "mailto", "tel"]);
        options.UriAttributes.Add("href");

        // KeepChildNodes stays at its default (false): a disallowed tag is removed together with its
        // content, not unwrapped. The alternative (true) was tried and rejected - it also unwraps
        // <script>/<style>, leaving their text content behind as plain (inert, but still unwanted)
        // page text, which is worse than losing a stray disallowed wrapper's content.
        _sanitizer = new HtmlSanitizer(options);
        _sanitizer.PostProcessNode += OnPostProcessNode;
    }

    public string Sanitize(string html) => _sanitizer.Sanitize(html ?? string.Empty);

    private void OnPostProcessNode(object? sender, PostProcessNodeEventArgs e)
    {
        switch (e.Node)
        {
            case IHtmlAnchorElement anchor when anchor.Target == "_blank":
                anchor.RelationList.Add("noopener");
                anchor.RelationList.Add("noreferrer");
                break;

            case IHtmlImageElement image when !IsAllowedImageSource(image.GetAttribute("src")):
                image.RemoveAttribute("src");
                break;

            case IHtmlInlineFrameElement iframe when !IsAllowedIframeSource(iframe.GetAttribute("src")):
                iframe.Remove();
                break;
        }
    }

    // Absolute URLs are never allowed here, even http/https ones pointing elsewhere - only a
    // relative path under this installation's own configured public media root may be embedded.
    private bool IsAllowedImageSource(string? src) =>
        !string.IsNullOrWhiteSpace(src)
        && !Uri.TryCreate(src, UriKind.Absolute, out _)
        && src.StartsWith(_publicMediaRootPrefix, StringComparison.Ordinal);

    private static bool IsAllowedIframeSource(string? src) =>
        Uri.TryCreate(src, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps
        && string.Equals(uri.Host, YouTubeNoCookieHost, StringComparison.OrdinalIgnoreCase)
        && uri.AbsolutePath.StartsWith(YouTubeNoCookieEmbedPathPrefix, StringComparison.Ordinal);
}
