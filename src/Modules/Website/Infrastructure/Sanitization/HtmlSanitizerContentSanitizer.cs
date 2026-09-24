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

    // Tags the whitelist deliberately excludes (div/span/section/article/font - never had semantic
    // meaning worth preserving) or didn't add every level of (h1/h5/h6 - only h2-h4 made AllowedTags)
    // but whose CONTENT is still safe once their own attributes have gone through the same
    // sanitization pass as everything else. Unwrapped, not removed with their content - unlike
    // script/style/noscript/template/object/embed/form/svg/math/disallowed-iframe, which stay fully
    // removed (see OnRemovingTag remarks).
    private static readonly HashSet<string> UnwrappableTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "div", "span", "section", "article", "font", "h1", "h5", "h6",
    };

    private readonly HtmlSanitizer _sanitizer;
    private readonly string _publicMediaRootPrefix;
    private readonly string? _publicBaseUrlPrefix;

    public HtmlSanitizerContentSanitizer(IOptions<FileStorageSettings> fileStorageOptions)
    {
        var fileStorageSettings = fileStorageOptions.Value;
        _publicMediaRootPrefix = fileStorageSettings.PublicRequestPath.TrimEnd('/') + "/";

        // The media library's own GetUrlAsync always returns an absolute "{PublicBaseUrl}/{fileKey}"
        // URL (ADR-019), so an editor inserting an image it just picked from that library writes an
        // absolute URL, not a relative one - only this installation's own configured PublicBaseUrl is
        // ever accepted as such and normalized back to a relative path (see NormalizeImageSource).
        _publicBaseUrlPrefix = string.IsNullOrWhiteSpace(fileStorageSettings.PublicBaseUrl)
            ? null
            : fileStorageSettings.PublicBaseUrl.TrimEnd('/') + "/";

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
        // content unless OnRemovingTag below chooses to unwrap it instead. Turning this flag on
        // globally was tried and rejected - it also unwraps <script>/<style>, leaving their text
        // content behind as plain (inert, but still unwanted) page text.
        _sanitizer = new HtmlSanitizer(options);
        _sanitizer.RemovingTag += OnRemovingTag;
        _sanitizer.PostProcessNode += OnPostProcessNode;
    }

    public string Sanitize(string html) => _sanitizer.Sanitize(html ?? string.Empty);

    // The sanitizer's own "remove disallowed tag" pass (RemoveReason.NotAllowedTag) would otherwise
    // drop harmless wrappers together with their safe content. Cancel just for those and unwrap them
    // by hand, the same way HtmlSanitizer's own KeepChildNodes flag would - but scoped to this list so
    // script/style/noscript/template/object/embed/form/svg/math (also NotAllowedTag, since none of
    // them are in AllowedTags) still lose their content along with the tag itself.
    private void OnRemovingTag(object? sender, RemovingTagEventArgs e)
    {
        if (e.Reason != RemoveReason.NotAllowedTag || !UnwrappableTags.Contains(e.Tag.NodeName))
        {
            return;
        }

        e.Cancel = true;

        if (e.Tag.HasChildNodes)
        {
            e.Tag.Replace([.. e.Tag.ChildNodes]);
        }
        else
        {
            e.Tag.Remove();
        }
    }

    private void OnPostProcessNode(object? sender, PostProcessNodeEventArgs e)
    {
        switch (e.Node)
        {
            case IHtmlAnchorElement anchor:
                ProcessAnchor(anchor);
                break;

            case IHtmlImageElement image:
                ProcessImage(image);
                break;

            case IHtmlInlineFrameElement iframe when !IsAllowedIframeSource(iframe.GetAttribute("src")):
                iframe.Remove();
                break;
        }
    }

    // A protocol-relative href ("//evil.com/x") carries no scheme token, so the library's own scheme
    // check - built to recognize things like "javascript:" or "https:" - never inspects it and lets it
    // through untouched, as if it were an ordinary in-site relative link. A browser instead resolves
    // it against the current page's own scheme, i.e. treats it exactly like an absolute URL to another
    // host. Reject it here since only href's declared AllowedSchemes (http/https/mailto/tel) are meant
    // to reach an absolute destination.
    private static void ProcessAnchor(IHtmlAnchorElement anchor)
    {
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

    private void ProcessImage(IHtmlImageElement image)
    {
        var normalized = NormalizeImageSource(image.GetAttribute("src"));
        if (normalized is null)
        {
            // A disallowed src does not become an image with no src - it becomes no image at all,
            // so nothing (a broken-image icon, an editor-added alt/caption around an empty image)
            // survives to suggest an image was ever there.
            image.Remove();
            return;
        }

        image.SetAttribute("src", normalized);
    }

    // Accepts either a relative path already under this installation's configured public media root,
    // or an absolute URL matching this installation's own configured PublicBaseUrl - the latter is
    // normalized back to a relative path so stored content never hardcodes a host/scheme (and still
    // resolves correctly behind a different host, e.g. after a domain change). Any other absolute URL
    // (a tracking pixel, an image hotlinked from elsewhere) is rejected.
    private string? NormalizeImageSource(string? src)
    {
        if (string.IsNullOrWhiteSpace(src))
        {
            return null;
        }

        if (_publicBaseUrlPrefix is not null && src.StartsWith(_publicBaseUrlPrefix, StringComparison.Ordinal))
        {
            var fileKey = src[_publicBaseUrlPrefix.Length..];
            return fileKey.Length == 0 ? null : _publicMediaRootPrefix + fileKey;
        }

        if (Uri.TryCreate(src, UriKind.Absolute, out _))
        {
            return null;
        }

        return src.StartsWith(_publicMediaRootPrefix, StringComparison.Ordinal) ? src : null;
    }

    private static bool IsAllowedIframeSource(string? src) =>
        Uri.TryCreate(src, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps
        && string.Equals(uri.Host, YouTubeNoCookieHost, StringComparison.OrdinalIgnoreCase)
        && uri.AbsolutePath.StartsWith(YouTubeNoCookieEmbedPathPrefix, StringComparison.Ordinal);
}
