using GenclikMerkezi.BuildingBlocks.Infrastructure.FileStorage;
using GenclikMerkezi.Modules.Website.Infrastructure.Sanitization;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.Website.Infrastructure.Sanitization;

public class HtmlSanitizerContentSanitizerTests
{
    private const string PublicBaseUrl = "http://localhost:5289/webuploads";

    private readonly HtmlSanitizerContentSanitizer _sanitizer = new(Options.Create(new FileStorageSettings()));

    // A second instance with PublicBaseUrl configured, matching a real deployment (appsettings.json
    // always sets one) - the default-settings instance above leaves it empty on purpose, to prove the
    // absolute-URL acceptance path is opt-in and never activates by accident.
    private readonly HtmlSanitizerContentSanitizer _sanitizerWithPublicBaseUrl = new(
        Options.Create(new FileStorageSettings { PublicBaseUrl = PublicBaseUrl }));

    [Fact]
    public void Sanitize_WithAllowedTagsAndFormatting_PreservesThem()
    {
        const string html = "<p>Merhaba <strong>dünya</strong>, <em>hoş geldiniz</em>.</p><ul><li>Bir</li><li>İki</li></ul>";

        var result = _sanitizer.Sanitize(html);

        Assert.Contains("<strong>dünya</strong>", result);
        Assert.Contains("<em>hoş geldiniz</em>", result);
        Assert.Contains("<li>Bir</li>", result);
    }

    [Fact]
    public void Sanitize_RemovesScriptTagEntirely()
    {
        const string html = "<p>Merhaba</p><script>alert('xss')</script>";

        var result = _sanitizer.Sanitize(html);

        Assert.Contains("Merhaba", result);
        Assert.DoesNotContain("<script", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("alert", result);
    }

    [Fact]
    public void Sanitize_RemovesOnErrorAttribute_ButKeepsAllowedImageSource()
    {
        const string html = "<img src=\"/webuploads/2026/09/24/logo.png\" onerror=\"alert(1)\" alt=\"Logo\">";

        var result = _sanitizer.Sanitize(html);

        Assert.DoesNotContain("onerror", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/webuploads/2026/09/24/logo.png", result);
    }

    [Fact]
    public void Sanitize_RemovesJavascriptHrefLink_ButKeepsLinkText()
    {
        const string html = "<a href=\"javascript:alert(1)\">tıkla</a>";

        var result = _sanitizer.Sanitize(html);

        Assert.DoesNotContain("javascript:", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("tıkla", result);
    }

    [Fact]
    public void Sanitize_RemovesDataUrlHref()
    {
        const string html = "<a href=\"data:text/html,<script>alert(1)</script>\">tıkla</a>";

        var result = _sanitizer.Sanitize(html);

        Assert.DoesNotContain("data:", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sanitize_KeepsAllowedAbsoluteHref()
    {
        const string html = "<a href=\"https://example.org/page\">bağlantı</a>";

        var result = _sanitizer.Sanitize(html);

        Assert.Contains("https://example.org/page", result);
    }

    [Fact]
    public void Sanitize_RemovesStyleAttribute()
    {
        const string html = "<p style=\"color:red;background:url(javascript:alert(1))\">metin</p>";

        var result = _sanitizer.Sanitize(html);

        Assert.DoesNotContain("style", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("metin", result);
    }

    [Fact]
    public void Sanitize_AddsNoopenerNoreferrer_WhenTargetIsBlank()
    {
        const string html = "<a href=\"https://example.org\" target=\"_blank\">bağlantı</a>";

        var result = _sanitizer.Sanitize(html);

        Assert.Contains("noopener", result);
        Assert.Contains("noreferrer", result);
    }

    [Fact]
    public void Sanitize_RemovesDisallowedIframeEntirely()
    {
        const string html = "<p>önce</p><iframe src=\"https://evil.example.com/steal\"></iframe><p>sonra</p>";

        var result = _sanitizer.Sanitize(html);

        Assert.DoesNotContain("<iframe", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("evil.example.com", result);
        Assert.Contains("önce", result);
        Assert.Contains("sonra", result);
    }

    [Fact]
    public void Sanitize_KeepsYouTubeNoCookieEmbedIframe()
    {
        const string html = "<iframe src=\"https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ\"></iframe>";

        var result = _sanitizer.Sanitize(html);

        Assert.Contains("youtube-nocookie.com/embed/dQw4w9WgXcQ", result);
    }

    [Fact]
    public void Sanitize_RemovesImageSourceOutsideThePublicMediaRoot()
    {
        const string html = "<img src=\"https://tracker.example.com/pixel.gif\" alt=\"\">";

        var result = _sanitizer.Sanitize(html);

        Assert.DoesNotContain("tracker.example.com", result);
    }

    [Fact]
    public void Sanitize_RemovesImageWithDisallowedSourceEntirely_NotJustTheSrcAttribute()
    {
        const string html = "<img src=\"https://tracker.example.com/pixel.gif\" alt=\"izlenen\">";

        var result = _sanitizer.Sanitize(html);

        Assert.DoesNotContain("<img", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sanitize_WithAbsoluteUrlFromOwnMediaLibrary_KeepsImageAndNormalizesToRelativePath()
    {
        // The exact shape LocalDiskFileStorageService.GetUrlAsync returns: "{PublicBaseUrl}/{fileKey}"
        // (ADR-019), which is what the editor writes when an image is picked from the media library.
        const string html = "<img src=\"http://localhost:5289/webuploads/public/2026/09/24/logo.webp\" alt=\"Logo\">";

        var result = _sanitizerWithPublicBaseUrl.Sanitize(html);

        Assert.Contains("src=\"/webuploads/public/2026/09/24/logo.webp\"", result);
        Assert.DoesNotContain("localhost:5289", result);
    }

    [Fact]
    public void Sanitize_WithAbsoluteUrlFromAnotherHost_RemovesImageEvenWhenPublicBaseUrlIsConfigured()
    {
        const string html = "<img src=\"https://tracker.example.com/webuploads/public/2026/09/24/logo.webp\" alt=\"\">";

        var result = _sanitizerWithPublicBaseUrl.Sanitize(html);

        Assert.DoesNotContain("<img", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tracker.example.com", result);
    }

    [Theory]
    [InlineData("<div><p>metin</p></div>")]
    [InlineData("<span>metin</span>")]
    public void Sanitize_UnwrapsHarmlessWrapperTag_ButKeepsItsSafeContent(string html)
    {
        var result = _sanitizer.Sanitize(html);

        Assert.DoesNotContain("<div", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<span", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("metin", result);
    }

    [Fact]
    public void Sanitize_UnwrapsNestedHarmlessWrapperTags_KeepingAllowedContentAtEachLevel()
    {
        const string html = "<div><section><p>önce</p><span>sonra</span></section></div>";

        var result = _sanitizer.Sanitize(html);

        Assert.DoesNotContain("<div", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<section", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<span", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<p>önce</p>", result);
        Assert.Contains("sonra", result);
    }

    [Fact]
    public void Sanitize_RemovesScriptTagAndItsContent_EvenThoughOtherWrappersAreUnwrapped()
    {
        const string html = "<p>Merhaba</p><script>alert('xss')</script>";

        var result = _sanitizer.Sanitize(html);

        Assert.Contains("Merhaba", result);
        Assert.DoesNotContain("<script", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("alert", result);
    }

    [Fact]
    public void Sanitize_RemovesStyleTagAndItsContent_EvenThoughOtherWrappersAreUnwrapped()
    {
        const string html = "<p>Merhaba</p><style>body { color: red; }</style>";

        var result = _sanitizer.Sanitize(html);

        Assert.Contains("Merhaba", result);
        Assert.DoesNotContain("<style", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("color: red", result);
    }

    [Fact]
    public void Sanitize_RemovesProtocolRelativeHref_ButKeepsLinkText()
    {
        const string html = "<a href=\"//evil.example.com/steal\">tıkla</a>";

        var result = _sanitizer.Sanitize(html);

        Assert.DoesNotContain("evil.example.com", result);
        Assert.Contains("tıkla", result);
    }
}
