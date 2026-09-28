using GenclikMerkezi.Modules.Website.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenclikMerkezi.Modules.Website.Infrastructure.Seed;

// ADR-024 §4.1 (Faz 1a Görev 2): this project's 13 initial content types, tr (default+active) and
// en (currently inactive, but seeded from day one so activating "en" later needs no data migration).
// Kept in its own class, separate from ContentTypeConfiguration, so another project can swap or empty
// this list without touching the mapping itself.
internal static class GenclikMerkeziContentTypeSeed
{
    private static readonly Guid SeedUserId = Guid.Empty;
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly IReadOnlyList<SeedType> Types =
    [
        new("page", "list", "page", ContentTypeSortMode.Manual,
            Flags("Hierarchy", "DetailImage", "Gallery", "Videos", "Attachments", "BlockLayout", "Form", "DetailPage", "Searchable"),
            new("Sayfa", string.Empty), new("Page", string.Empty)),

        new("news", "cards", "article", ContentTypeSortMode.PublishDateDesc,
            Flags("Categories", "Tags", "DetailImage", "Gallery", "Videos", "Attachments", "RelatedContent", "DetailPage", "ListingPage", "Searchable"),
            new("Haber", "haberler"), new("News", "news")),

        new("announcement", "list", "article", ContentTypeSortMode.PublishDateDesc,
            Flags("Categories", "Attachments", "RelatedContent", "DetailPage", "ListingPage", "Searchable"),
            new("Duyuru", "duyurular"), new("Announcement", "announcements")),

        new("project", "cards", "project", ContentTypeSortMode.Manual,
            Flags("Categories", "Tags", "DetailImage", "Gallery", "Videos", "Attachments", "Form", "RelatedContent", "DetailPage", "ListingPage", "Searchable"),
            new("Proje", "projeler"), new("Project", "projects")),

        new("activity", "cards", "article", ContentTypeSortMode.PublishDateDesc,
            Flags("Categories", "Tags", "DetailImage", "Gallery", "Videos", "RelatedContent", "DetailPage", "ListingPage", "Searchable"),
            new("Faaliyet", "faaliyetler"), new("Activity", "activities")),

        new("success-story", "cards", "story", ContentTypeSortMode.PublishDateDesc,
            Flags("Tags", "DetailImage", "Gallery", "Videos", "RelatedContent", "DetailPage", "ListingPage", "Searchable"),
            new("Başarı Hikayesi", "basari-hikayeleri"), new("Success Story", "success-stories")),

        new("event", "cards", "event", ContentTypeSortMode.EventDateAsc,
            Flags("Categories", "Tags", "DetailImage", "Gallery", "Videos", "Attachments", "Event", "RelatedContent", "DetailPage", "ListingPage", "Searchable"),
            new("Etkinlik", "etkinlikler"), new("Event", "events")),

        new("training", "cards", "event", ContentTypeSortMode.EventDateAsc,
            Flags("Categories", "Tags", "DetailImage", "Gallery", "Videos", "Attachments", "Event", "RelatedContent", "DetailPage", "ListingPage", "Searchable"),
            new("Eğitim ve Atölye", "egitimler"), new("Training & Workshop", "trainings")),

        new("volunteer-opportunity", "cards", "opportunity", ContentTypeSortMode.PublishDateDesc,
            Flags("Categories", "DetailImage", "Form", "RelatedContent", "DetailPage", "ListingPage", "Searchable"),
            new("Gönüllülük Fırsatı", "gonulluluk-firsatlari"), new("Volunteer Opportunity", "volunteer-opportunities")),

        new("faq", "faq-accordion", "faq", ContentTypeSortMode.Manual,
            Flags("Categories", "ListingPage", "Searchable"),
            new("SSS", "sss"), new("FAQ", "faq")),

        new("team", "team-grid", "team", ContentTypeSortMode.Manual,
            Flags("Categories", "ListingPage"),
            new("Ekip", "ekip"), new("Team", "team")),

        new("document", "document-list", "document", ContentTypeSortMode.PublishDateDesc,
            Flags("Categories", "Attachments", "ListingPage", "Searchable"),
            new("Belge", "belgeler"), new("Document", "documents")),

        new("press-release", "list", "article", ContentTypeSortMode.PublishDateDesc,
            Flags("DetailImage", "Attachments", "RelatedContent", "DetailPage", "ListingPage", "Searchable"),
            new("Basın Bülteni", "basin-bultenleri"), new("Press Release", "press-releases")),
    ];

    // Flags' 14 fields are listed flat, top-level, rather than nested as "Flags = type.Flags": EF
    // Core's HasData silently drops a complex property provided as one nested object (verified against
    // a real migration - the generated InsertData omitted every Flags column entirely and the seed
    // insert failed NOT NULL constraints), so each field must appear as its own top-level member here,
    // matching its flattened column exactly.
    public static void ApplyContentTypes(EntityTypeBuilder<ContentType> builder) =>
        builder.HasData(Types.Select((type, index) => new
        {
            Id = ContentTypeId(type.Key),
            Key = ContentTypeKey.Create(type.Key).Value,
            ListTemplate = type.ListTemplate,
            DetailTemplate = type.DetailTemplate,
            SortMode = type.SortMode,
            IsActive = true,
            SortOrder = index + 1,
            type.Flags.SupportsHierarchy,
            type.Flags.SupportsCategories,
            type.Flags.SupportsTags,
            type.Flags.SupportsDetailImage,
            type.Flags.SupportsGallery,
            type.Flags.SupportsVideos,
            type.Flags.SupportsAttachments,
            type.Flags.SupportsEvent,
            type.Flags.SupportsBlockLayout,
            type.Flags.SupportsForm,
            type.Flags.SupportsRelatedContent,
            type.Flags.HasDetailPage,
            type.Flags.HasListingPage,
            type.Flags.IsSearchable,
            type.Flags.RequiresReview,
            RowVersion = DeterministicGuid.Create($"ContentType:{type.Key}:RowVersion").ToByteArray(),
            CreatedByUserId = SeedUserId,
            CreatedAtUtc = SeedTimestamp,
        }));

    public static void ApplyTranslations(OwnedNavigationBuilder<ContentType, ContentTypeTranslation> translation) =>
        translation.HasData(Types.SelectMany(type => new[]
        {
            TranslationRow(type, "tr", type.Tr),
            TranslationRow(type, "en", type.En),
        }));

    // SeoMetadata is a second-level owned type (OwnsOne inside Translations' OwnsMany) sharing its
    // owner's primary key by convention, so each row here keys off the exact same Id a
    // ContentTypeTranslation row above was given for the same type+language - no separate identity of
    // its own. Every seed type starts with empty SEO metadata (the master prompt's table has none).
    public static void ApplySeo(OwnedNavigationBuilder<ContentTypeTranslation, SeoMetadata> seo) =>
        seo.HasData(Types.SelectMany(type => new[]
        {
            SeoRow(TranslationId(type.Key, "tr")),
            SeoRow(TranslationId(type.Key, "en")),
        }));

    private static object TranslationRow(SeedType type, string languageCode, SeedTranslation values) => new
    {
        Id = TranslationId(type.Key, languageCode),
        ContentTypeId = ContentTypeId(type.Key),
        LanguageCode = LanguageCode.Create(languageCode).Value,
        Name = values.Name,
        RoutePrefix = values.RoutePrefix,
    };

    private static object SeoRow(Guid translationId) => new
    {
        ContentTypeTranslationId = translationId,
        MetaTitle = string.Empty,
        MetaDescription = string.Empty,
        MetaKeywords = string.Empty,
        OgTitle = string.Empty,
        OgDescription = string.Empty,
        OgImageMediaId = (Guid?)null,
        CanonicalUrl = (string?)null,
        NoIndex = false,
    };

    private static Guid ContentTypeId(string key) => DeterministicGuid.Create($"ContentType:{key}");

    private static Guid TranslationId(string key, string languageCode) => DeterministicGuid.Create($"ContentType:{key}:{languageCode}");

    private static ContentTypeFeatureFlags Flags(params string[] enabled)
    {
        var set = new HashSet<string>(enabled, StringComparer.Ordinal);

        return new ContentTypeFeatureFlags(
            SupportsHierarchy: set.Contains("Hierarchy"),
            SupportsCategories: set.Contains("Categories"),
            SupportsTags: set.Contains("Tags"),
            SupportsDetailImage: set.Contains("DetailImage"),
            SupportsGallery: set.Contains("Gallery"),
            SupportsVideos: set.Contains("Videos"),
            SupportsAttachments: set.Contains("Attachments"),
            SupportsEvent: set.Contains("Event"),
            SupportsBlockLayout: set.Contains("BlockLayout"),
            SupportsForm: set.Contains("Form"),
            SupportsRelatedContent: set.Contains("RelatedContent"),
            HasDetailPage: set.Contains("DetailPage"),
            HasListingPage: set.Contains("ListingPage"),
            IsSearchable: set.Contains("Searchable"),
            RequiresReview: false);
    }

    private sealed record SeedTranslation(string Name, string RoutePrefix);

    private sealed record SeedType(
        string Key,
        string ListTemplate,
        string DetailTemplate,
        ContentTypeSortMode SortMode,
        ContentTypeFeatureFlags Flags,
        SeedTranslation Tr,
        SeedTranslation En);
}
