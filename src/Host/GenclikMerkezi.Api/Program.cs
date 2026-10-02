using System.Security.Claims;
using System.Threading.RateLimiting;
using GenclikMerkezi.Admin;
using GenclikMerkezi.Api;
using GenclikMerkezi.Api.Hangfire;
using GenclikMerkezi.Api.Website;
using GenclikMerkezi.BuildingBlocks.Infrastructure.DependencyInjection;
using GenclikMerkezi.BuildingBlocks.Infrastructure.ExceptionHandling;
using GenclikMerkezi.BuildingBlocks.Infrastructure.FileStorage;
using GenclikMerkezi.Contracts.Website;
using GenclikMerkezi.Modules.Candidate;
using GenclikMerkezi.Modules.Candidate.Infrastructure.DependencyInjection;
using GenclikMerkezi.Modules.CareerAdvisor;
using GenclikMerkezi.Modules.CareerAdvisor.Infrastructure.DependencyInjection;
using GenclikMerkezi.Modules.CareerDevelopment;
using GenclikMerkezi.Modules.CareerDevelopment.Infrastructure.DependencyInjection;
using GenclikMerkezi.Modules.Employer;
using GenclikMerkezi.Modules.Employer.Infrastructure.DependencyInjection;
using GenclikMerkezi.Modules.Employment;
using GenclikMerkezi.Modules.Employment.Infrastructure.DependencyInjection;
using GenclikMerkezi.Modules.Identity;
using GenclikMerkezi.Modules.Identity.Infrastructure;
using GenclikMerkezi.Modules.Identity.Infrastructure.DependencyInjection;
using GenclikMerkezi.Modules.Interview;
using GenclikMerkezi.Modules.Interview.Infrastructure.DependencyInjection;
using GenclikMerkezi.Modules.Matching;
using GenclikMerkezi.Modules.Matching.Infrastructure.DependencyInjection;
using GenclikMerkezi.Modules.Notification;
using GenclikMerkezi.Modules.Notification.Infrastructure.DependencyInjection;
using GenclikMerkezi.Modules.ReferenceData;
using GenclikMerkezi.Modules.ReferenceData.Infrastructure.DependencyInjection;
using GenclikMerkezi.Modules.Support;
using GenclikMerkezi.Modules.Support.Infrastructure.DependencyInjection;
using GenclikMerkezi.Modules.Support.Infrastructure.Jobs;
using GenclikMerkezi.Modules.Website;
using GenclikMerkezi.Modules.Website.Infrastructure.DependencyInjection;
using GenclikMerkezi.Modules.Website.Infrastructure.Jobs;
using Hangfire;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Declared this early so both the Data Protection block below and the rate-limiting/Hangfire
// configuration further down can read it - IntegrationTests' CustomWebApplicationFactory sets the
// "Testing" environment via UseEnvironment, never DPAPI-protects its throwaway keys (DPAPI keys are
// bound to the machine/user profile running the test, which CI runners may not have), and redirects
// FileStorage/Data Protection paths into a per-factory temp directory instead of this project's
// App_Data (SECURITY.md §23.2 / ADR-024 §4.5).
var isTestingEnvironment = builder.Environment.IsEnvironment("Testing");

builder.Services.AddOpenApi();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSharedApplicationServices(
    typeof(IdentityModuleMarker).Assembly,
    typeof(NotificationModuleMarker).Assembly,
    typeof(ReferenceDataModuleMarker).Assembly,
    typeof(CandidateModuleMarker).Assembly,
    typeof(CareerAdvisorModuleMarker).Assembly,
    typeof(EmployerModuleMarker).Assembly,
    typeof(MatchingModuleMarker).Assembly,
    typeof(InterviewModuleMarker).Assembly,
    typeof(EmploymentModuleMarker).Assembly,
    typeof(CareerDevelopmentModuleMarker).Assembly,
    typeof(SupportModuleMarker).Assembly,
    typeof(WebsiteModuleMarker).Assembly);

// ADR-017: ICacheService (and, once registered, IUserScopedCacheService) - shared, not per-module,
// so it is registered here rather than inside any single AddXModule().
builder.Services.AddCaching(builder.Configuration);

// ADR-019: IFileStorageService - shared, not per-module, same reasoning as AddCaching above.
builder.Services.AddFileStorage(builder.Configuration);

builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddNotificationModule(builder.Configuration);
builder.Services.AddReferenceDataModule(builder.Configuration);
builder.Services.AddCandidateModule(builder.Configuration);
builder.Services.AddCareerAdvisorModule(builder.Configuration);
builder.Services.AddEmployerModule(builder.Configuration);
builder.Services.AddMatchingModule(builder.Configuration);
builder.Services.AddInterviewModule(builder.Configuration);
builder.Services.AddEmploymentModule(builder.Configuration);
builder.Services.AddCareerDevelopmentModule(builder.Configuration);
builder.Services.AddSupportModule(builder.Configuration);
builder.Services.AddWebsiteModule(builder.Configuration);

// ADR-024 §1 Görev 7: Website's IWebsiteEmailSender port, wired at the Host composition root to
// Notification's public contract - Website itself depends on neither. Unlike this, Website's
// IBotProtectionVerifier port (Cloudflare Turnstile) needs no Host adapter: it is registered
// directly by AddWebsiteModule, since it never touches another business module (ARCHITECTURE.md
// §50.1).
builder.Services.AddScoped<IWebsiteEmailSender, NotificationWebsiteEmailSender>();

// ADR-024 §4.5 (Faz 1b Görev 6): signed, time-limited content preview link tokens use ASP.NET Core
// Data Protection. Keys are persisted under App_Data (never under webuploads' public static-file
// root) so an IIS application pool recycle does not invalidate every outstanding preview link - the
// default (in-memory/registry) key storage would not survive a recycle. The directory is
// configurable (DataProtection:KeyDirectory, defaulting to App_Data/dataprotection-keys) so
// CustomWebApplicationFactory can redirect it into a per-test-run temp folder instead of writing
// into this project's own source tree (a prior test run leaked an unencrypted key file into git -
// see SECURITY.md §23.2).
var dataProtectionKeyDirectory = Path.Combine(
    builder.Environment.ContentRootPath,
    builder.Configuration["DataProtection:KeyDirectory"] ?? Path.Combine("App_Data", "dataprotection-keys"));

var dataProtectionBuilder = builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeyDirectory))
    .SetApplicationName("GenclikMerkezi");

// SECURITY.md §23.2: on Windows (IIS/on-prem deployment target - ADR-024 §1), keys are additionally
// encrypted at rest with DPAPI, scoped to the application pool's user profile ("Load User Profile =
// True" is required in IIS for this to work across process recycles). Skipped in the Testing
// environment: DPAPI keys are bound to the machine/user profile that encrypted them, which a CI
// runner cannot be assumed to retain between runs, and these are throwaway keys anyway.
if (OperatingSystem.IsWindows() && !isTestingEnvironment)
{
    dataProtectionBuilder.ProtectKeysWithDpapi();
}

// ADR-024 Faz 1b Görev 1: canlı ortam (Turhost/IIS) reverse proxy'siz çalıştığı için varsayılan
// olarak kapalı - Enabled=true iken KnownProxies/KnownNetworks'ten en az biri dolu olmalı, aksi halde
// herkes kendi IP'sini X-Forwarded-For ile sahteleyip rate limiting'i atlatabilir. .ValidateOnStart()
// bu kontrolü uygulama başlarken (ilk isteği beklemeden) çalıştırır.
builder.Services.AddOptions<ReverseProxySettings>()
    .Bind(builder.Configuration.GetSection(ReverseProxySettings.SectionName))
    .Validate(
        settings => !settings.Enabled || settings.KnownProxies.Count > 0 || settings.KnownNetworks.Count > 0,
        "ReverseProxy:Enabled is true but both KnownProxies and KnownNetworks are empty - configure at " +
        "least one trusted proxy address or network, or X-Forwarded-For could be spoofed by any client.")
    .ValidateOnStart();

// Part 0 (Hangfire altyapısı): tüm modüllerin yeniden kullanabileceği genel bir background-job
// altyapısı - herhangi bir modüle ait değil, Host'ta bir kez kaydedilir (CAP/AddMessaging ile aynı
// gerekçe). Kendi başına, herhangi bir modülün sahipliğinde olmayan bir infrastructure DB kullanır.
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireDatabase")));

// The actual background *server* (a real worker thread pool, distinct from the storage/client
// registration above) is skipped for the "Testing" environment: no integration test needs Hangfire
// to actually process a job, and every WebApplicationFactory<Program> instance in the test run
// starts (and later disposes) its own server against the same shared Hangfire storage database.
// Its shutdown signal handler logs through Hangfire.AspNetCore.AspNetCoreLog -> ILogger, and on
// this host the default Windows EventLog logging provider can already be disposed by then, which
// throws ObjectDisposedException on a raw ThreadPool callback thread with nothing to catch it -
// that crashes the whole test process outright (observed once as "Etkin test çalıştırması iptal
// edildi... Cannot access a disposed object. Object name: 'EventLogInternal'."). Not starting the
// server in tests removes the only code path that triggers it.
if (!isTestingEnvironment)
{
    builder.Services.AddHangfireServer();
}

// Host-seviyesi çok-modüllü orkestrasyon (ADR-022 §1) - herhangi bir modüle ait değil, bu yüzden
// modüllerin AddXModule() metotlarının hiçbirinde değil, burada kaydediliyor.
builder.Services.AddScoped<CareerAdvisorDeactivationOrchestrator>();

// CAP supports exactly one instance per process, so it is registered exactly once here rather
// than inside each module's own AddXModule() - IdentityDbContext is the transactional outbox
// anchor since Identity is the system's first publisher (ADR-014's amendment). Any [CapSubscribe]
// consumer registered by any module (e.g. Notification's) is still discovered by this one
// registration regardless of which assembly it lives in.
builder.Services.AddMessaging<IdentityDbContext>(builder.Configuration, builder.Environment);

// The "Testing" environment (integration tests) shares a single in-process host across many
// requests from multiple test cases; the production limits would cause unrelated test failures.
// PublicReadPermitLimit is additionally configuration-driven (rather than a plain ternary like the
// other two) so a dedicated test can override just this one policy's limit to something small and
// deterministic (e.g. to prove X-Forwarded-For partitioning) without touching the shared "Testing"
// default every other integration test relies on.
var credentialEndpointPermitLimit = isTestingEnvironment ? 1000 : 5;
var authenticatedEndpointPermitLimit = isTestingEnvironment ? 1000 : 60;
var publicReadEndpointPermitLimit = builder.Configuration.GetValue(
    "RateLimiting:PublicReadPermitLimit", isTestingEnvironment ? 1000 : 300);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Login/Register/ForgotPassword/ResetPassword/VerifyEmail/ResendVerificationEmail: partitioned
    // per client IP, 5/min in production. Prevents credential-stuffing/brute-force attempts
    // regardless of which account is targeted. ResendVerificationEmail additionally enforces a
    // one-per-minute-per-account cooldown of its own (User.RequestEmailVerificationResend) since
    // this IP policy alone would not stop repeated resends aimed at a single victim account from
    // different IPs.
    options.AddPolicy("auth", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: GetClientIpAddress(httpContext),
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = credentialEndpointPermitLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
        }));

    // RefreshAccessToken/GetCurrentUser/ChangePassword/etc.: partitioned per authenticated user
    // when available, falling back to client IP for anonymous requests (e.g. token refresh).
    options.AddPolicy("authenticated", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: GetAuthenticatedPartitionKey(httpContext),
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = authenticatedEndpointPermitLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
        }));

    // Anonymous public GET reads (public site, route resolution, and Faz 1b's new public listing/
    // detail/video endpoints): partitioned per client IP, a higher limit than "authenticated" since a
    // single page view fans out into several of these calls (ADR-024 Faz 1b Görev 1).
    options.AddPolicy("public-read", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: GetClientIpAddress(httpContext),
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = publicReadEndpointPermitLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
        }));
});

static string GetClientIpAddress(HttpContext httpContext) =>
    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

static string GetAuthenticatedPartitionKey(HttpContext httpContext) =>
    httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? GetClientIpAddress(httpContext);

var app = builder.Build();

app.UseExceptionHandler();

// ADR-024 Faz 1b Görev 1: kapalıyken (varsayılan) hiç uygulanmaz - RemoteIpAddress zaten gerçek
// ziyaretçi IP'sidir. Açıksa yalnızca burada listelenen adreslerden/ağlardan (canlıda: proxy'nin
// kendisi) gelen X-Forwarded-For kabul edilir; middleware zaten bunun dışındaki bağlantılardan gelen
// header'ı yok sayar. En erken middleware olarak çalışması gerekir - rate limiting dahil, IP'ye bakan
// her şeyden önce.
var reverseProxySettings = app.Services.GetRequiredService<IOptions<ReverseProxySettings>>().Value;
if (reverseProxySettings.Enabled)
{
    app.UseForwardedHeaders(ReverseProxyForwardedHeadersOptionsFactory.Create(reverseProxySettings));
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "GenclikMerkezi API v1"));
}

app.UseHttpsRedirection();

// ADR-019 Ek (ADR-024 Faz 0 Görev 4): only the PUBLIC storage root is ever served statically, at
// PublicRequestPath, with a long-lived immutable cache (file names are GUIDs - a given URL's
// content never changes) and nosniff. The private root (candidate photos/CVs, employer documents,
// website form attachments) has no static file mapping at all here or anywhere else in this file -
// there is deliberately no way to reach it over HTTP yet (see ADR-024 Faz 0 master prompt's
// "kapsam dışı" list: authorized download endpoints for it are follow-up work).
var publicFileStorageSettings = app.Services.GetRequiredService<IOptions<FileStorageSettings>>().Value;
var publicFileStorageRoot = Path.GetFullPath(
    Path.Combine(app.Environment.ContentRootPath, publicFileStorageSettings.PublicRootDirectory));
Directory.CreateDirectory(publicFileStorageRoot);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(publicFileStorageRoot),
    RequestPath = publicFileStorageSettings.PublicRequestPath,
    OnPrepareResponse = context =>
    {
        context.Context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
    },
});

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// SECURITY.md §37: dashboard yalnızca Admin rolüne açık - UseAuthorization()'dan SONRA map edilir ki
// HangfireAdminDashboardAuthorizationFilter'ın okuduğu HttpContext.User dolu olsun.
app.MapHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAdminDashboardAuthorizationFilter()],
});

app.MapIdentityModuleEndpoints();
app.MapReferenceDataModuleEndpoints();
app.MapCandidateModuleEndpoints();
app.MapCareerAdvisorModuleEndpoints();
app.MapAdminCareerAdvisorEndpoints();
app.MapEmployerModuleEndpoints();
app.MapMatchingModuleEndpoints();
app.MapInterviewModuleEndpoints();
app.MapEmploymentModuleEndpoints();
app.MapCareerDevelopmentModuleEndpoints();
app.MapSupportModuleEndpoints();
app.MapWebsiteModuleEndpoints();

// Support'un tek Hangfire tüketicisi olduğu bu aşamada, ayrı bir IRecurringJobScheduler soyutlaması
// yerine doğrudan burada kaydedilir (aşırı soyutlama yapma - AGENTS.md §51). İleride başka modüller
// de kendi recurring job'larını aynı şekilde burada (ya da kendi Program.cs eklentisinde) kaydedebilir.
// Skipped for "Testing" along with AddHangfireServer() above - no server is running to execute it,
// and RecurringJob.AddOrUpdate would otherwise write against the shared Hangfire storage database
// from every parallel test host.
if (!isTestingEnvironment)
{
    RecurringJob.AddOrUpdate<CloseOverdueSupportTicketsJob>(
        "support-close-overdue-tickets", job => job.ExecuteAsync(CancellationToken.None), Cron.Hourly);
    RecurringJob.AddOrUpdate<CleanupStaleNotFoundLogsJob>(
        "website-cleanup-stale-not-found-logs", job => job.ExecuteAsync(CancellationToken.None), Cron.Daily);
    RecurringJob.AddOrUpdate<CleanupUnusedContentTagsJob>(
        "website-cleanup-unused-content-tags", job => job.ExecuteAsync(CancellationToken.None), Cron.Daily);
    RecurringJob.AddOrUpdate<PermanentlyDeleteExpiredTrashJob>(
        "website-permanently-delete-expired-trash", job => job.ExecuteAsync(CancellationToken.None), Cron.Daily);
}

app.Run();

public partial class Program;
