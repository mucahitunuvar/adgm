using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Application.ThirdPartyScripts;

// ADR-024 §13 (Faz 3 Görev 7): shared by CreateThirdPartyScript and UpdateThirdPartyScript instead of
// duplicating the same "which typed factory does this request's Provider string map to" switch in both
// handlers (mirrors FormDefinitionLegalReferenceGuard's shape). allowedScriptHosts is the Application
// layer's read of Website:AllowedScriptHosts, handed to ThirdPartyScriptProvider.CreateExternalScript as
// plain data.
public static class ThirdPartyScriptProviderResolver
{
    public static Result<ThirdPartyScriptProvider> Resolve(
        string? providerKind,
        string? measurementId,
        string? containerId,
        string? pixelId,
        string? src,
        bool async,
        bool defer,
        IReadOnlyList<string> allowedScriptHosts)
    {
        if (!Enum.TryParse<ThirdPartyScriptProviderKind>(providerKind, out var kind))
        {
            return Result.Failure<ThirdPartyScriptProvider>(Error.Validation(
                "ThirdPartyScript.ProviderInvalid", "Provider must be one of 'GoogleAnalytics4', 'GoogleTagManager', 'MetaPixel', 'ExternalScript'."));
        }

        return kind switch
        {
            ThirdPartyScriptProviderKind.GoogleAnalytics4 => ThirdPartyScriptProvider.CreateGoogleAnalytics4(measurementId),
            ThirdPartyScriptProviderKind.GoogleTagManager => ThirdPartyScriptProvider.CreateGoogleTagManager(containerId),
            ThirdPartyScriptProviderKind.MetaPixel => ThirdPartyScriptProvider.CreateMetaPixel(pixelId),
            ThirdPartyScriptProviderKind.ExternalScript => ThirdPartyScriptProvider.CreateExternalScript(src, async, defer, allowedScriptHosts),
            _ => Result.Failure<ThirdPartyScriptProvider>(Error.Validation("ThirdPartyScript.ProviderInvalid", "Unrecognized provider.")),
        };
    }
}
