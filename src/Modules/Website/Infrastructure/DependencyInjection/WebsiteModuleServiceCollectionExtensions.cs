using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Application.BlockTypes;
using GenclikMerkezi.Modules.Website.Application.BlockTypes.Definitions;
using GenclikMerkezi.Modules.Website.Application.BlockTypes.PublicResolution;
using GenclikMerkezi.Modules.Website.Application.ContentPaths;
using GenclikMerkezi.Modules.Website.Application.ImpactMetrics;
using GenclikMerkezi.Modules.Website.Application.Forms;
using GenclikMerkezi.Modules.Website.Application.Forms.PublicResolution;
using GenclikMerkezi.Modules.Website.Application.LegalDocuments;
using GenclikMerkezi.Modules.Website.Application.LinkTargets;
using GenclikMerkezi.Modules.Website.Application.Media;
using GenclikMerkezi.Modules.Website.Application.Partners;
using GenclikMerkezi.Modules.Website.Application.PublicSubmissions;
using GenclikMerkezi.Modules.Website.Application.RouteResolution;
using GenclikMerkezi.Modules.Website.Application.Sliders;
using GenclikMerkezi.Modules.Website.Infrastructure;
using GenclikMerkezi.Modules.Website.Infrastructure.BotProtection;
using GenclikMerkezi.Modules.Website.Infrastructure.Jobs;
using GenclikMerkezi.Modules.Website.Infrastructure.Media;
using GenclikMerkezi.Modules.Website.Infrastructure.Newsletter;
using GenclikMerkezi.Modules.Website.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Website.Infrastructure.Preview;
using GenclikMerkezi.Modules.Website.Infrastructure.PublicSubmissions;
using GenclikMerkezi.Modules.Website.Infrastructure.Sanitization;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.Modules.Website.Infrastructure.DependencyInjection;

public static class WebsiteModuleServiceCollectionExtensions
{
    // ADR-024 §2 / AGENTS.md §1: literal "Admin" role string, same as every other module (ADR-016
    // pattern) - never a reference to Identity.Domain.UserRole, which Website must not depend on.
    private const string AdminRole = "Admin";

    public static IServiceCollection AddWebsiteModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<WebsiteDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("WebsiteDatabase");

            // ADR-012: Sqlite-for-Testing switch, same as every other module's AddXModule().
            if (string.Equals(configuration["Database:Provider"], "Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                options.UseSqlServer(connectionString);
            }
        });

        services.AddKeyedScoped<IUnitOfWork>(
            WebsiteModuleMarker.UnitOfWorkKey,
            (sp, _) => sp.GetRequiredService<WebsiteDbContext>());

        services.AddScoped<ISiteLanguageRepository, SiteLanguageRepository>();
        services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();
        services.AddScoped<ISiteSettingsRepository, SiteSettingsRepository>();
        services.AddScoped<IContentTypeRepository, ContentTypeRepository>();
        services.AddScoped<IContentItemRepository, ContentItemRepository>();
        services.AddScoped<IRedirectRepository, RedirectRepository>();
        services.AddScoped<INotFoundLogRepository, NotFoundLogRepository>();
        services.AddScoped<IVideoRepository, VideoRepository>();
        services.AddScoped<IContentCategoryRepository, ContentCategoryRepository>();
        services.AddScoped<IContentTagRepository, ContentTagRepository>();
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<ISliderRepository, SliderRepository>();
        services.AddScoped<IPartnerRepository, PartnerRepository>();
        services.AddScoped<IImpactMetricRepository, ImpactMetricRepository>();
        services.AddScoped<IPageLayoutRepository, PageLayoutRepository>();
        services.AddScoped<IPopupRepository, PopupRepository>();
        services.AddScoped<ILegalDocumentRepository, LegalDocumentRepository>();
        services.AddScoped<ILegalDocumentUsageChecker, LegalDocumentUsageChecker>();
        services.AddScoped<IFormDefinitionRepository, FormDefinitionRepository>();
        services.AddScoped<IFormDefinitionUsageChecker, FormDefinitionUsageChecker>();
        services.AddScoped<IFormSubmissionRepository, FormSubmissionRepository>();
        services.AddScoped<IFormSubmissionSequenceRepository, FormSubmissionSequenceRepository>();
        services.AddScoped<IPersonalDataAccessLogRepository, PersonalDataAccessLogRepository>();
        services.AddScoped<INewsletterSubscriberRepository, NewsletterSubscriberRepository>();
        services.AddScoped<IThirdPartyScriptRepository, ThirdPartyScriptRepository>();
        services.AddScoped<ICookieConsentRecordRepository, CookieConsentRecordRepository>();
        services.AddScoped<PublicFormDefinitionResolver>();
        services.AddScoped<LinkTargetResolver>();
        services.AddScoped<SliderPublicQueryService>();
        services.AddScoped<PartnerPublicQueryService>();
        services.AddScoped<ImpactMetricPublicQueryService>();
        services.AddScoped<PublicPageLayoutResolver>();
        services.AddScoped<ContentPathCascadeService>();
        services.AddScoped<RelatedContentResolutionService>();
        services.AddScoped<ContentItemPermanentDeletionService>();
        services.AddScoped<RouteResolutionService>();
        // CleanupStaleNotFoundLogsJob only takes singleton-safe dependencies (IServiceScopeFactory), so
        // it is registered Transient here and resolved by Program.cs's RecurringJob.AddOrUpdate<T>() -
        // the same pattern Support's CloseOverdueSupportTicketsJob already uses.
        services.AddTransient<CleanupStaleNotFoundLogsJob>();
        services.AddTransient<CleanupUnusedContentTagsJob>();
        services.AddTransient<PermanentlyDeleteExpiredTrashJob>();
        services.AddTransient<ArchiveClosedFormSubmissionsJob>();
        services.AddTransient<AnonymizeExpiredFormSubmissionsJob>();
        services.AddTransient<CleanupExpiredNewsletterSubscribersJob>();
        services.AddTransient<CleanupExpiredCookieConsentRecordsJob>();
        services.AddSingleton<IImageProcessor, SkiaSharpImageProcessor>();
        services.AddScoped<IMediaUsageChecker, CompositeMediaUsageChecker>();
        services.AddScoped<IMediaUsageProvider, SiteSettingsMediaUsageProvider>();
        services.AddScoped<IMediaUsageProvider, ContentItemMediaUsageProvider>();
        services.AddScoped<IMediaUsageProvider, VideoMediaUsageProvider>();
        services.AddScoped<IMediaUsageProvider, SliderMediaUsageProvider>();
        services.AddScoped<IMediaUsageProvider, PartnerMediaUsageProvider>();
        services.AddScoped<IMediaUsageProvider, LayoutMediaUsageProvider>();
        services.AddScoped<IMediaUsageProvider, PopupMediaUsageProvider>();
        services.AddScoped<IVideoUsageChecker, VideoUsageChecker>();
        services.AddScoped<ISliderUsageChecker, SliderUsageChecker>();
        services.AddScoped<PageLayoutReferenceScanner>();
        services.AddSingleton<IHtmlContentSanitizer, HtmlSanitizerContentSanitizer>();

        // §4.1 "Blok tipleri kodla tanımlanır": every IBlockTypeDefinition in the Faz 2 Görev 4
        // catalog, fanned into IBlockTypeRegistry the same way IMediaUsageProvider fans into
        // CompositeMediaUsageChecker.
        services.AddSingleton<IBlockTypeDefinition, HeroSliderBlockTypeDefinition>();
        services.AddSingleton<IBlockTypeDefinition, LogoStripBlockTypeDefinition>();
        services.AddSingleton<IBlockTypeDefinition, QuickLinksBlockTypeDefinition>();
        services.AddSingleton<IBlockTypeDefinition, ContentListBlockTypeDefinition>();
        services.AddSingleton<IBlockTypeDefinition, UpcomingEventsBlockTypeDefinition>();
        services.AddSingleton<IBlockTypeDefinition, JobListBlockTypeDefinition>();
        services.AddSingleton<IBlockTypeDefinition, FeatureMosaicBlockTypeDefinition>();
        services.AddSingleton<IBlockTypeDefinition, ProcessStepsBlockTypeDefinition>();
        services.AddSingleton<IBlockTypeDefinition, VideoFeatureBlockTypeDefinition>();
        services.AddSingleton<IBlockTypeDefinition, ImpactStatsBlockTypeDefinition>();
        services.AddSingleton<IBlockTypeDefinition, CtaBlockTypeDefinition>();
        services.AddSingleton<IBlockTypeDefinition, RichTextBlockTypeDefinition>();
        services.AddSingleton<IBlockTypeDefinition, ImageTextBlockTypeDefinition>();
        services.AddSingleton<IBlockTypeDefinition, FaqBlockTypeDefinition>();
        services.AddSingleton<IBlockTypeDefinition, GalleryBlockTypeDefinition>();
        services.AddSingleton<IBlockTypeRegistry, BlockTypeRegistry>();
        services.AddScoped<PageLayoutReferenceValidator>();
        services.AddScoped<LayoutBlockInputProcessor>();
        services.AddSingleton<IContentPreviewLinkGenerator, DataProtectionContentPreviewLinkGenerator>();

        // ADR-024 §1/§12.3 Görev 8: IBotProtectionVerifier's Cloudflare Turnstile implementation
        // lives here (not a Host adapter) since it never touches another business module -
        // ARCHITECTURE.md §50.1.
        services.Configure<TurnstileSettings>(configuration.GetSection(TurnstileSettings.SectionName));
        services.AddHttpClient<IBotProtectionVerifier, TurnstileBotProtectionVerifier>(client =>
        {
            client.BaseAddress = new Uri("https://challenges.cloudflare.com/");
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        // ADR-024 §12.3 (Faz 3 Görev 1): anonymous submission protection - the token generator is
        // Data Protection-backed (Infrastructure), the guard itself is pure orchestration over other
        // ports (Application), same split as IContentPreviewLinkGenerator/IMediaUsageChecker above.
        services.AddSingleton<ISubmissionTokenGenerator, DataProtectionSubmissionTokenGenerator>();
        services.AddScoped<IPublicSubmissionGuard, PublicSubmissionGuard>();

        // ADR-024 §14 (Faz 3 Görev 6): the newsletter double opt-in confirmation link - same Data
        // Protection backing as ISubmissionTokenGenerator/IContentPreviewLinkGenerator above.
        services.AddSingleton<INewsletterConfirmationLinkGenerator, DataProtectionNewsletterConfirmationLinkGenerator>();

        // ADR-024 §13 (Faz 3 Görev 7): the ExternalScript host allow-list - read as plain configuration
        // data and handed to ThirdPartyScriptProvider.CreateExternalScript by the Create/Update command
        // handlers, keeping the Domain layer itself free of any IConfiguration dependency.
        services.Configure<WebsiteScriptSettings>(configuration.GetSection(WebsiteScriptSettings.SectionName));

        // ADR-024 §2: named policies, all resolving to the literal Admin role for now. Only this
        // block changes when a real permission system arrives - endpoints stay untouched.
        services.AddAuthorization(options =>
        {
            options.AddPolicy(WebsitePolicies.ContentManage, policy => policy.RequireRole(AdminRole));
            options.AddPolicy(WebsitePolicies.ContentPublish, policy => policy.RequireRole(AdminRole));
            options.AddPolicy(WebsitePolicies.StructureManage, policy => policy.RequireRole(AdminRole));
            options.AddPolicy(WebsitePolicies.DesignManage, policy => policy.RequireRole(AdminRole));
            options.AddPolicy(WebsitePolicies.SettingsManage, policy => policy.RequireRole(AdminRole));
            options.AddPolicy(WebsitePolicies.SubmissionsView, policy => policy.RequireRole(AdminRole));
            options.AddPolicy(WebsitePolicies.SubmissionsManage, policy => policy.RequireRole(AdminRole));
        });

        return services;
    }
}
