namespace GenclikMerkezi.Modules.Website.Application.Abstractions;

// ADR-024 §13 (Faz 3 Görev 7): "alan adı Website:AllowedScriptHosts yapılandırma listesinde olmalı" -
// a plain options POCO the Create/UpdateThirdPartyScript command handlers (Application) read directly
// via IOptions<WebsiteScriptSettings>. It lives here, not in Infrastructure, precisely so those
// Application-layer handlers binding to it does not become an Application -> Infrastructure dependency
// (AGENTS.md §7) - only the binding itself (services.Configure<WebsiteScriptSettings>(...) in
// AddWebsiteModule) touches IConfiguration, mirroring how FormDefinitionLegalReferenceGuard's own
// abstractions live in Application while their wiring lives in Infrastructure/DI.
public sealed class WebsiteScriptSettings
{
    public const string SectionName = "Website:Scripts";

    public IReadOnlyList<string> AllowedScriptHosts { get; init; } = [];
}
