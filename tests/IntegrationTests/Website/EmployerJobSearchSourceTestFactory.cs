using GenclikMerkezi.Contracts.Employer;
using GenclikMerkezi.IntegrationTests.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §10 (Faz 5 Görev 4). Overrides IPublishedJobModuleContract with FakeContract the same way
// CustomWebApplicationFactory itself overrides IEmailSender/IBotProtectionVerifier (a later
// registration wins for single-service resolution, no RemoveAll needed) - every other module's
// wiring (including the real EmployerJobSearchSource adapter that consumes this port) stays exactly
// as Program.cs sets it up.
public sealed class EmployerJobSearchSourceTestFactory : CustomWebApplicationFactory
{
    public FakePublishedJobModuleContract FakeContract { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services => services.AddScoped<IPublishedJobModuleContract>(_ => FakeContract));
    }
}
