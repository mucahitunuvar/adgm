namespace GenclikMerkezi.Modules.Website.Features.UpdateThirdPartyScriptTranslation;

public sealed record UpdateThirdPartyScriptTranslationRequest(byte[] RowVersion, string? Name, string? Purpose);
