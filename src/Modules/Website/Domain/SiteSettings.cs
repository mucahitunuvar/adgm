using GenclikMerkezi.SharedKernel.Domain;

namespace GenclikMerkezi.Modules.Website.Domain;

// ADR-024 §13. Singleton aggregate: exactly one row, at the well-known Id below. There is
// deliberately no factory that creates "a" SiteSettings with a random id - only CreateDefault(),
// used both to seed the very first persisted row (see each grouped UpdateSiteSettings* command
// handler's get-or-create) and to hand read-only callers sensible defaults, without writing
// anything, when no row has ever been persisted yet (GetSiteSettingsQueryHandler /
// GetPublicSiteQueryHandler never call SaveChanges - a query must not commit a transaction,
// AGENTS.md §13).
//
// Görev 6's grouped PUT endpoints (identity, theme, contact, bank-accounts, features, maintenance,
// bot-protection) each update one independent slice of this aggregate and each takes RowVersion for
// optimistic concurrency - two admins editing different sections at the same time must not silently
// clobber each other. RowVersion is a plain application-managed token (Touch() regenerates it on
// every mutation), not a database-generated rowversion/timestamp column: Website runs on both
// SqlServer and Sqlite (ADR-012), and Sqlite has no equivalent auto-updating column type.
public sealed class SiteSettings : AggregateRoot
{
    public static readonly Guid SingletonId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly List<SocialLink> _socialLinks = [];
    private readonly List<BankAccount> _bankAccounts = [];
    private readonly List<SiteSettingsTranslation> _translations = [];

    // "Identity" group (ADR-024 §13 Görev 6): logo/favicon/OG-image are loose MediaAsset references
    // (by id, resolved to a URL at read time), the same reasoning as every other cross-aggregate
    // reference in this module - never a navigation, since MediaAsset lives in its own aggregate
    // boundary. SiteName/Tagline/DefaultMetaTitle/DefaultMetaDescription/FooterText live per-language
    // on SiteSettingsTranslation instead, since identity is also part of what changes by language.
    public Guid? LogoLightMediaAssetId { get; private set; }

    public Guid? LogoDarkMediaAssetId { get; private set; }

    public Guid? FaviconMediaAssetId { get; private set; }

    public Guid? DefaultOgImageMediaId { get; private set; }

    public SiteTheme Theme { get; private set; } = null!;

    public ContactInfo Contact { get; private set; } = null!;

    public IReadOnlyList<SocialLink> SocialLinks => _socialLinks.AsReadOnly();

    public IReadOnlyList<BankAccount> BankAccounts => _bankAccounts.AsReadOnly();

    public IReadOnlyList<SiteSettingsTranslation> Translations => _translations.AsReadOnly();

    public bool GlobalSearchEnabled { get; private set; }

    public bool NewsletterEnabled { get; private set; }

    public bool PublicJobListingsEnabled { get; private set; }

    public bool DonationPageEnabled { get; private set; }

    public bool BotProtectionEnabled { get; private set; }

    public bool MaintenanceModeEnabled { get; private set; }

    // ADR-024 §13: the public (non-secret) Turnstile key the frontend widget embeds. The secret key
    // is never stored here - it lives only in the Website Infrastructure adapter's configuration.
    public string TurnstileSiteKey { get; private set; } = string.Empty;

    public byte[] RowVersion { get; private set; } = Guid.NewGuid().ToByteArray();

    public Guid? UpdatedByUserId { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    private SiteSettings(
        Guid id,
        SiteTheme theme,
        ContactInfo contact,
        bool globalSearchEnabled,
        bool newsletterEnabled,
        bool publicJobListingsEnabled,
        bool donationPageEnabled,
        bool botProtectionEnabled)
        : base(id)
    {
        Theme = theme;
        Contact = contact;
        GlobalSearchEnabled = globalSearchEnabled;
        NewsletterEnabled = newsletterEnabled;
        PublicJobListingsEnabled = publicJobListingsEnabled;
        DonationPageEnabled = donationPageEnabled;
        BotProtectionEnabled = botProtectionEnabled;
        MaintenanceModeEnabled = false;
    }

    private SiteSettings()
    {
    }

    // ADR-024 §13: donation page/global search/newsletter/public job listings all start disabled in
    // this project (no online payment, no search index yet, no newsletter sender, Employer's public
    // listing work is out of this ADR's scope). Bot protection starts enabled since every anonymous
    // POST endpoint must be guarded from the day it ships (Görev 8 wires the real Turnstile check).
    public static SiteSettings CreateDefault() => new(
        SingletonId, SiteTheme.CreateEmpty(), ContactInfo.CreateEmpty(),
        globalSearchEnabled: false, newsletterEnabled: false, publicJobListingsEnabled: false,
        donationPageEnabled: false, botProtectionEnabled: true);

    public void UpdateIdentity(
        Guid? logoLightMediaAssetId, Guid? logoDarkMediaAssetId, Guid? faviconMediaAssetId, Guid? defaultOgImageMediaId,
        Guid updatedByUserId, DateTime updatedAtUtc)
    {
        LogoLightMediaAssetId = logoLightMediaAssetId;
        LogoDarkMediaAssetId = logoDarkMediaAssetId;
        FaviconMediaAssetId = faviconMediaAssetId;
        DefaultOgImageMediaId = defaultOgImageMediaId;
        Touch(updatedByUserId, updatedAtUtc);
    }

    // Upserts the identity text fields for languageCode without touching that language's maintenance
    // message (SetMaintenanceMessage's job) - one call per language the caller wants to set.
    public void SetIdentityTranslation(
        LanguageCode languageCode,
        string? siteName,
        string? tagline,
        string? defaultMetaTitle,
        string? defaultMetaDescription,
        string? footerText,
        Guid updatedByUserId,
        DateTime updatedAtUtc)
    {
        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is not null)
        {
            existing.UpdateIdentityFields(siteName, tagline, defaultMetaTitle, defaultMetaDescription, footerText);
        }
        else
        {
            _translations.Add(SiteSettingsTranslation.Create(
                languageCode, siteName, tagline, defaultMetaTitle, defaultMetaDescription, footerText, null));
        }

        Touch(updatedByUserId, updatedAtUtc);
    }

    public void UpdateTheme(SiteTheme theme, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        Theme = theme;
        Touch(updatedByUserId, updatedAtUtc);
    }

    public void UpdateContact(ContactInfo contact, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        Contact = contact;
        Touch(updatedByUserId, updatedAtUtc);
    }

    public void ReplaceSocialLinks(IReadOnlyList<SocialLink> socialLinks, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        _socialLinks.Clear();
        _socialLinks.AddRange(socialLinks);
        Touch(updatedByUserId, updatedAtUtc);
    }

    public void ReplaceBankAccounts(IReadOnlyList<BankAccount> bankAccounts, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        _bankAccounts.Clear();
        _bankAccounts.AddRange(bankAccounts);
        Touch(updatedByUserId, updatedAtUtc);
    }

    // Upserts the maintenance message for languageCode without touching that language's identity
    // fields (SetIdentityTranslation's job).
    public void SetMaintenanceMessage(LanguageCode languageCode, string? message, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        var existing = _translations.FirstOrDefault(t => t.LanguageCode == languageCode);
        if (existing is not null)
        {
            existing.UpdateMaintenanceMessage(message);
        }
        else
        {
            _translations.Add(SiteSettingsTranslation.Create(languageCode, null, null, null, null, null, message));
        }

        Touch(updatedByUserId, updatedAtUtc);
    }

    public void UpdateFeatureFlags(
        bool globalSearchEnabled,
        bool newsletterEnabled,
        bool publicJobListingsEnabled,
        bool donationPageEnabled,
        Guid updatedByUserId,
        DateTime updatedAtUtc)
    {
        GlobalSearchEnabled = globalSearchEnabled;
        NewsletterEnabled = newsletterEnabled;
        PublicJobListingsEnabled = publicJobListingsEnabled;
        DonationPageEnabled = donationPageEnabled;
        Touch(updatedByUserId, updatedAtUtc);
    }

    public void UpdateBotProtection(bool botProtectionEnabled, string? turnstileSiteKey, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        BotProtectionEnabled = botProtectionEnabled;
        TurnstileSiteKey = (turnstileSiteKey ?? string.Empty).Trim();
        Touch(updatedByUserId, updatedAtUtc);
    }

    // Global on/off switch only - the message itself is per-language (SetMaintenanceMessage), since a
    // maintenance banner must speak the visitor's language.
    public void SetMaintenanceMode(bool enabled, Guid updatedByUserId, DateTime updatedAtUtc)
    {
        MaintenanceModeEnabled = enabled;
        Touch(updatedByUserId, updatedAtUtc);
    }

    private void Touch(Guid updatedByUserId, DateTime updatedAtUtc)
    {
        UpdatedByUserId = updatedByUserId;
        UpdatedAtUtc = updatedAtUtc;
        RowVersion = Guid.NewGuid().ToByteArray();
    }
}
