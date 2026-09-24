using GenclikMerkezi.BuildingBlocks.Infrastructure.FileStorage;
using GenclikMerkezi.Modules.Website.Infrastructure.Sanitization;
using Microsoft.Extensions.Options;

namespace GenclikMerkezi.UnitTests.Website.Infrastructure.Sanitization;

public class HtmlSanitizerContentSanitizerTests
{
    private readonly HtmlSanitizerContentSanitizer _sanitizer = new(Options.Create(new FileStorageSettings()));

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
    public void Sanitize_RemovesDisallowedTagTogetherWithItsContent()
    {
        // KeepChildNodes stays false (the library's own default): unwrapping disallowed tags instead
        // would also unwrap <script>/<style>, leaving their text content behind as inert but
        // unwanted page text (verified by hand while building this policy - see
        // Sanitize_RemovesScriptTagEntirely for the case that ruled it out).
        const string html = "<p>önce</p><div>izinsiz sarmalayıcı içeriği</div><p>sonra</p>";

        var result = _sanitizer.Sanitize(html);

        Assert.DoesNotContain("<div", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("izinsiz sarmalayıcı içeriği", result);
        Assert.Contains("önce", result);
        Assert.Contains("sonra", result);
    }
}
