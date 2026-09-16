namespace GenclikMerkezi.Modules.Notification.Application;

// Used to build links inside transactional emails (email verification, password reset).
// Points at the API itself for now (no frontend yet, per ADR-010's Backend-First order) -
// update to the frontend's own base URL once it exists. Lives in Application (not
// Infrastructure) because Features/SendVerificationEmail's handler consumes it directly to
// build link text (AGENTS.md §7: Application must not depend on Infrastructure) - Infrastructure
// only binds it from configuration via IOptions<AppLinkSettings>.
public sealed class AppLinkSettings
{
    public const string SectionName = "App";

    public string ApiBaseUrl { get; init; } = string.Empty;
}
