using System.Text.RegularExpressions;
using GenclikMerkezi.SharedKernel.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §13 (Faz 3 Görev 7): "Ham HTML/JavaScript kabul edilmez... tipli sağlayıcılar" - the same
// "one VO, several factories, empty elsewhere" shape PopupTargeting/LinkTarget already use. Exactly
// one group of fields is populated, matching Kind; the other three stay at their default. ExternalScript's
// allowed-host check takes the configured allow-list as plain data (allowedHosts) rather than reading
// IConfiguration itself, keeping this value object free of any Infrastructure dependency (AGENTS.md §7)
// while still enforcing the invariant inside the aggregate - the same reasoning
// LegalDocumentEffectiveVersionResolver documents for taking `now` as a parameter instead of reading
// TimeProvider itself.
public sealed partial class ThirdPartyScriptProvider : ValueObject
{
    public const int MaxSrcLength = 1000;

    public ThirdPartyScriptProviderKind Kind { get; }

    public string? MeasurementId { get; }

    public string? ContainerId { get; }

    public string? PixelId { get; }

    public string? Src { get; }

    public bool Async { get; }

    public bool Defer { get; }

    private ThirdPartyScriptProvider(
        ThirdPartyScriptProviderKind kind, string? measurementId, string? containerId, string? pixelId, string? src, bool async, bool defer)
    {
        Kind = kind;
        MeasurementId = measurementId;
        ContainerId = containerId;
        PixelId = pixelId;
        Src = src;
        Async = async;
        Defer = defer;
    }

    public static Result<ThirdPartyScriptProvider> CreateGoogleAnalytics4(string? measurementId)
    {
        var normalized = (measurementId ?? string.Empty).Trim().ToUpperInvariant();
        if (!GoogleAnalytics4Pattern().IsMatch(normalized))
        {
            return Result.Failure<ThirdPartyScriptProvider>(Error.Validation(
                "ThirdPartyScriptProvider.MeasurementIdInvalid", "Measurement id must match 'G-' followed by 4 to 12 letters/digits."));
        }

        return Result.Success(new ThirdPartyScriptProvider(
            ThirdPartyScriptProviderKind.GoogleAnalytics4, normalized, null, null, null, false, false));
    }

    public static Result<ThirdPartyScriptProvider> CreateGoogleTagManager(string? containerId)
    {
        var normalized = (containerId ?? string.Empty).Trim().ToUpperInvariant();
        if (!GoogleTagManagerPattern().IsMatch(normalized))
        {
            return Result.Failure<ThirdPartyScriptProvider>(Error.Validation(
                "ThirdPartyScriptProvider.ContainerIdInvalid", "Container id must match 'GTM-' followed by 4 to 10 letters/digits."));
        }

        return Result.Success(new ThirdPartyScriptProvider(
            ThirdPartyScriptProviderKind.GoogleTagManager, null, normalized, null, null, false, false));
    }

    public static Result<ThirdPartyScriptProvider> CreateMetaPixel(string? pixelId)
    {
        var normalized = (pixelId ?? string.Empty).Trim();
        if (!MetaPixelPattern().IsMatch(normalized))
        {
            return Result.Failure<ThirdPartyScriptProvider>(Error.Validation(
                "ThirdPartyScriptProvider.PixelIdInvalid", "Pixel id must be a 10 to 20 digit number."));
        }

        return Result.Success(new ThirdPartyScriptProvider(ThirdPartyScriptProviderKind.MetaPixel, null, null, normalized, null, false, false));
    }

    // §13 "alan adı Website:AllowedScriptHosts yapılandırma listesinde olmalı" - allowedHosts is the
    // Application layer's read of that configuration, passed in as data.
    public static Result<ThirdPartyScriptProvider> CreateExternalScript(
        string? src, bool async, bool defer, IReadOnlyList<string> allowedHosts)
    {
        var trimmed = (src ?? string.Empty).Trim();

        if (trimmed.Length == 0
            || trimmed.Length > MaxSrcLength
            || !Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps)
        {
            return Result.Failure<ThirdPartyScriptProvider>(Error.Validation(
                "ThirdPartyScriptProvider.SrcInvalid", $"Src must be an absolute https URL of at most {MaxSrcLength} characters."));
        }

        if (!allowedHosts.Any(allowed => string.Equals(allowed, uri.Host, StringComparison.OrdinalIgnoreCase)))
        {
            return Result.Failure<ThirdPartyScriptProvider>(Error.Validation(
                "ThirdPartyScriptProvider.HostNotAllowed", $"Host '{uri.Host}' is not in the allowed script host list."));
        }

        return Result.Success(new ThirdPartyScriptProvider(ThirdPartyScriptProviderKind.ExternalScript, null, null, null, trimmed, async, defer));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Kind;
        yield return MeasurementId;
        yield return ContainerId;
        yield return PixelId;
        yield return Src;
        yield return Async;
        yield return Defer;
    }

    [GeneratedRegex("^G-[A-Z0-9]{4,12}$")]
    private static partial Regex GoogleAnalytics4Pattern();

    [GeneratedRegex("^GTM-[A-Z0-9]{4,10}$")]
    private static partial Regex GoogleTagManagerPattern();

    [GeneratedRegex(@"^\d{10,20}$")]
    private static partial Regex MetaPixelPattern();
}
