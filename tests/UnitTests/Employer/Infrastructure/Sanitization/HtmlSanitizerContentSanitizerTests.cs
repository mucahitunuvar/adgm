using GenclikMerkezi.Modules.Employer.Infrastructure.Sanitization;

namespace GenclikMerkezi.UnitTests.Employer.Infrastructure.Sanitization;

// Görev 2 (Employer public jobs master prompt). Deliberately smaller whitelist than Website's own
// HtmlSanitizerContentSanitizerTests (no img/iframe - AboutHtml has no media-embedding feature).
public class HtmlSanitizerContentSanitizerTests
{
    private readonly HtmlSanitizerContentSanitizer _sanitizer = new();

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
    public void Sanitize_RemovesScriptTagAndItsContentEntirely()
    {
        const string html = "<p>Merhaba</p><script>alert('xss')</script>";

        var result = _sanitizer.Sanitize(html);

        Assert.Contains("Merhaba", result);
        Assert.DoesNotContain("<script", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("alert", result);
    }

    [Fact]
    public void Sanitize_RemovesImageTagEntirely_NoMediaEmbeddingFeatureExists()
    {
        const string html = "<p>Önce</p><img src=\"https://example.com/logo.png\" onerror=\"alert(1)\"><p>Sonra</p>";

        var result = _sanitizer.Sanitize(html);

        Assert.DoesNotContain("<img", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Önce", result);
        Assert.Contains("Sonra", result);
    }

    [Fact]
    public void Sanitize_RemovesIframeTagEntirely()
    {
        const string html = "<p>önce</p><iframe src=\"https://evil.example.com/steal\"></iframe>";

        var result = _sanitizer.Sanitize(html);

        Assert.DoesNotContain("<iframe", result, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("evil.example.com", result);
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
    public void Sanitize_RemovesProtocolRelativeHref_ButKeepsLinkText()
    {
        const string html = "<a href=\"//evil.example.com/steal\">tıkla</a>";

        var result = _sanitizer.Sanitize(html);

        Assert.DoesNotContain("evil.example.com", result);
        Assert.Contains("tıkla", result);
    }

    [Fact]
    public void Sanitize_KeepsAllowedAbsoluteHref()
    {
        const string html = "<a href=\"https://example.org/page\">bağlantı</a>";

        var result = _sanitizer.Sanitize(html);

        Assert.Contains("https://example.org/page", result);
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
    public void Sanitize_RemovesStyleAttribute()
    {
        const string html = "<p style=\"color:red;background:url(javascript:alert(1))\">metin</p>";

        var result = _sanitizer.Sanitize(html);

        Assert.DoesNotContain("style", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("metin", result);
    }

    [Fact]
    public void Sanitize_WithEmptyOrNullInput_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, _sanitizer.Sanitize(string.Empty));
        Assert.Equal(string.Empty, _sanitizer.Sanitize(null!));
    }
}
