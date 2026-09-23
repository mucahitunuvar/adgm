using System.Security.Claims;
using System.Threading.RateLimiting;
using GenclikMerkezi.Admin;
using GenclikMerkezi.Api.Hangfire;
using GenclikMerkezi.BuildingBlocks.Infrastructure.DependencyInjection;
using GenclikMerkezi.BuildingBlocks.Infrastructure.ExceptionHandling;
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
using Hangfire;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

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

var isTestingEnvironment = builder.Environment.IsEnvironment("Testing");

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
var credentialEndpointPermitLimit = isTestingEnvironment ? 1000 : 5;
var authenticatedEndpointPermitLimit = isTestingEnvironment ? 1000 : 60;

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
});

static string GetClientIpAddress(HttpContext httpContext) =>
    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

static string GetAuthenticatedPartitionKey(HttpContext httpContext) =>
    httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? GetClientIpAddress(httpContext);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "GenclikMerkezi API v1"));
}

app.UseHttpsRedirection();

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
}

app.Run();

public partial class Program;
