using Hangfire.Dashboard;

namespace GenclikMerkezi.Api.Hangfire;

// SECURITY.md §37: Hangfire dashboard production'da public bırakılmamalı, Authentication + Authorization
// ile korunmalı. Host-seviyesi composition-root parçası (herhangi bir modüle ait değil), Program.cs'in
// UseAuthentication()/UseAuthorization() middleware'lerinden sonra çalışır - HttpContext.User burada
// zaten doldurulmuş olur.
public sealed class HangfireAdminDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    private const string AdminRole = "Admin";

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        return httpContext.User.Identity is { IsAuthenticated: true } && httpContext.User.IsInRole(AdminRole);
    }
}
